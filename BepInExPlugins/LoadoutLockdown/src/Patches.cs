using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.LoadoutLockdown;


[HarmonyPatch]
public static unsafe class Patches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;
    private static LoadoutLockdownService LoadoutService => LoadoutLockdownService.Instance;

    // todo: cache things. should be on a service probably
    private static NativeParallelHashMap<PrefabGUID, ItemData> ItemHashLookupMap => WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>().ItemHashLookupMap;
    private static ServerScriptMapper ServerScriptMapper => WorldUtil.Server.GetExistingSystemManaged<ServerScriptMapper>();
    private static ServerRootPrefabCollection ServerRootPrefabCollection => ServerScriptMapper.GetSingleton<ServerRootPrefabCollection>();
    private static NetworkIdLookupMap NetworkIdLookupMap => ServerScriptMapper.GetSingleton<NetworkIdSystem.Singleton>()._NetworkIdLookupMap;

    private const bool SKIP_ORIGINAL_METHOD = false;
    private const bool EXECUTE_ORIGINAL_METHOD = true;

    [HarmonyPatch(typeof(InventoryUtilitiesServer), nameof(InventoryUtilitiesServer.TryAddItem), new Type[] { typeof(AddItemSettings), typeof(Entity), typeof(PrefabGUID), typeof(int) })]
    [HarmonyPrefix]
    public static void TryAddItem_Prefix(ref AddItemSettings addItemSettings, Entity target, PrefabGUID itemType, int amount)
    {
        if (!ItemHashLookupMap.TryGetValue(itemType, out var itemData))
        {
            return;
        }

        if (LoadoutService.IsEquipmentForbidden(itemData.Entity))
        {
            addItemSettings.EquipIfPossible = false;
        }
    }

    [HarmonyPatch(typeof(InventoryUtilitiesServer), nameof(InventoryUtilitiesServer.TryAddItem), new Type[] { typeof(AddItemSettings), typeof(Entity), typeof(InventoryBuffer) })]
    [HarmonyPrefix]
    public static void TryAddItem_Prefix(ref AddItemSettings addItemSettings, Entity target, InventoryBuffer inventoryItem)
    {
        if (!ItemHashLookupMap.TryGetValue(inventoryItem.ItemType, out var itemData))
        {
            return;
        }

        if (LoadoutService.IsEquipmentForbidden(itemData.Entity))
        {
            addItemSettings.EquipIfPossible = false;
        }
    }

    // note: the original IsValidWeaponEquip is not just a check. it has side effects: moving the item into an open/junk slot
    // We completely disable this, and do our own processing to fix jank messaging / side effects.
    // We still automatically move weapons into valid weapon slots, but with different rules about which slots can be moved into.
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidWeaponEquip))]
    [HarmonyPrefix]
    public static bool IsValidWeaponEquip_Prefix(ref bool __result, EntityManager entityManager, EquippableData equippableData, EquipItemEvent equipItem, ServerRootPrefabCollection serverRootPrefabs, Entity character, NativeParallelHashMap<PrefabGUID, ItemData> itemHashLookupMap, int weaponSlots)
    {
        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }
        //__result = LoadoutService.IsValidItemEquip(character, character, equipItem.SlotIndex, equipItem.IsCosmetic);
        __result = true;
        return SKIP_ORIGINAL_METHOD;
    }

    // "IsValidItemMove" covers the case of moving an item between two inventory slots. They can be slots in different inventories.
    // It does NOT cover the case of moving stuff into a designated slot (e.g. moving a cloak into the cloak slot).
    //
    // The original IsValidItemMove internally calls IsValidItemTransfer twice. once each for the orderings of slotA and slotB.
    // i.e. the second time IsValidItemTransfer is called, the former slotA will appear as slotB, and the former slotB will appear as slotA.
    //
    // We do not call IsValidItemMove at all, unless falling back to the original behaviour.
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidItemMove))]
    [HarmonyPrefix]
    public static unsafe bool IsValidItemMove_Prefix(
        ref bool __result,
        EntityManager entityManager,
        ServerRootPrefabCollection serverRootPrefabCollection,
        NativeParallelHashMap<PrefabGUID, ItemData> itemDataMap,
        MoveItemBetweenInventoriesEvent moveEvent,
        Entity toInventory,
        Entity fromInventory,
        int weaponSlots
    )
    {
        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        bool isFromPlayerInventory = LoadoutService.IsPlayerInventory(fromInventory);
        bool isToPlayerInventory = LoadoutService.IsPlayerInventory(toInventory);

        Entity playerCharacter = default;
        bool foundPlayerCharacter = false;
        if (isFromPlayerInventory)
        {
            foundPlayerCharacter = LoadoutService.TryGetOwnerOfInventory(fromInventory, out playerCharacter);
        }
        else if (isToPlayerInventory)
        {
            foundPlayerCharacter = LoadoutService.TryGetOwnerOfInventory(toInventory, out playerCharacter);
        }

        if (!foundPlayerCharacter)
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        bool isInPvPCombat = NewWeaponEquipmentRestrictionsUtility.IsInPvPCombat(EntityManager, serverRootPrefabCollection, playerCharacter);
        bool fromWeaponSlot = isFromPlayerInventory && LoadoutService.IsValidWeaponSlot(moveEvent.FromSlot);
        bool toWeaponSlot = isToPlayerInventory && LoadoutService.IsValidWeaponSlot(moveEvent.ToSlot);
        bool doesNotInvolveWeaponSlot = !fromWeaponSlot && !toWeaponSlot;
        bool isRearrangingWeaponSlots = fromWeaponSlot && toWeaponSlot;

        if (!isInPvPCombat || doesNotInvolveWeaponSlot || isRearrangingWeaponSlots)
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        InventoryBuffer menuSlotIB;
        InventoryBuffer weaponSlotIB;
        bool isMenuSlotEmpty;
        bool isWeaponSlotEmpty;
        if (fromWeaponSlot)
        {
            isWeaponSlotEmpty = !InventoryUtilities.TryGetItemAtSlot(EntityManager, fromInventory, moveEvent.FromSlot, out weaponSlotIB);
            isMenuSlotEmpty = !InventoryUtilities.TryGetItemAtSlot(EntityManager, toInventory, moveEvent.ToSlot, out menuSlotIB);
        }
        else
        {
            isMenuSlotEmpty = !InventoryUtilities.TryGetItemAtSlot(EntityManager, fromInventory, moveEvent.FromSlot, out menuSlotIB);
            isWeaponSlotEmpty = !InventoryUtilities.TryGetItemAtSlot(EntityManager, toInventory, moveEvent.ToSlot, out weaponSlotIB);
        }

        if (isMenuSlotEmpty && isWeaponSlotEmpty)
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        bool doesItemInMenuHaveDesignatedSlot = !isMenuSlotEmpty && LoadoutService.HasDesignatedSlot(menuSlotIB.ItemEntity._Entity);

        if (!isMenuSlotEmpty && !doesItemInMenuHaveDesignatedSlot && LoadoutService.AlwaysAllowSwapIntoSlot(menuSlotIB.ItemEntity._Entity))
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        if (!isMenuSlotEmpty && !doesItemInMenuHaveDesignatedSlot && LoadoutService.CanMenuSwapIntoFilledSlotDuringPVP(menuSlotIB.ItemEntity._Entity))
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        if (isWeaponSlotEmpty || LoadoutService.IsWasteInWeaponSlot(weaponSlotIB))
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        if (isMenuSlotEmpty && LoadoutService.IsWasteInWeaponSlot(weaponSlotIB))
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        __result = false;
        return SKIP_ORIGINAL_METHOD;
    }

    // todo: cover dropping items? Does not seem critical though - dropping items in combat is really risky.
    // Might be a fun "exploit" - high risk, questionable reward.
    //
    // IsValidItemDrop never seems to be called.
    // I'm guessing WeaponSlots was a half-cooked feature that got cut,
    // hence not appearing in any game menu or server settings documentation.
    // Something else prevents hotbar items being dropped in combat.
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidItemDrop))]
    [HarmonyPrefix]
    public static unsafe bool IsValidItemDrop_Prefix(
        ref bool __result,
        EntityManager entityManager,
        Entity characterEntity,
        int slotId,
        ServerRootPrefabCollection serverRootPrefabs
    )
    {
        LogUtil.LogDebug("running IsValidItemDrop prefix");

        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        if (!InventoryUtilities.TryGetItemAtSlot(EntityManager, characterEntity, slotIndex: slotId, out InventoryBuffer itemInSlot))
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        bool isInPvPCombat = NewWeaponEquipmentRestrictionsUtility.IsInPvPCombat(EntityManager, serverRootPrefabs, characterEntity);

        if (!isInPvPCombat || !LoadoutService.IsValidWeaponSlot(slotId) || LoadoutService.IsWasteInWeaponSlot(itemInSlot))
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        __result = false;
        return SKIP_ORIGINAL_METHOD;
    }

    // TryUnEquipAndAddItem covers the case of unequipping an item from a designated slot,
    // and moving it into an inventory. The inventory can be any inventory, not just the player's main inventory.
    [HarmonyPatch(typeof(InventoryUtilitiesServer), nameof(InventoryUtilitiesServer.TryUnEquipAndAddItem))]
    [HarmonyPrefix]
    public static bool TryUnEquipAndAddItem_Prefix(
        ref bool __result,
        EntityManager entityManager,
        NativeParallelHashMap<PrefabGUID, ItemData> itemDataMap,
        Entity target,
        Entity inventoryOwnerEntity,
        int toSlotIndex,
        Entity item,
        out bool addedToInventory,
        Il2CppSystem.Nullable_Unboxed<EntityCommandBuffer> commandBuffer
    )
    {
        addedToInventory = false;

        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        bool isInPvPCombat = NewWeaponEquipmentRestrictionsUtility.IsInPvPCombat(EntityManager, ServerRootPrefabCollection, target);
        if (!isInPvPCombat || LoadoutService.CanDirectlyMoveOutOfSlotDuringPVP(item))
        {
            // allow unequipping; execute original method to do the unequip.
            return EXECUTE_ORIGINAL_METHOD;
        }

        // do not allow unequipping
        LoadoutService.SendMessageCannotMenuSwapDuringPVP(target);
        __result = false;
        return SKIP_ORIGINAL_METHOD;
    }


    // EquipItemFromInventorySystem covers the case of items being dragged into a designated slot from an inventory.
    // The inventory could be an external inventory, or the player's own inventory.
    [HarmonyPatch(typeof(EquipItemFromInventorySystem), nameof(EquipItemFromInventorySystem.OnUpdate))]
    [HarmonyPrefix]
    public static void EquipItemFromInventorySystem_OnUpdate_Prefix(EquipItemFromInventorySystem __instance)
    {
        if (LoadoutService is null)
        {
            return;
        }

        var entities = __instance._Query.ToEntityArray(Allocator.Temp);
        var equipItemFromInventoryEvents = __instance._Query.ToComponentDataArray<EquipItemFromInventoryEvent>(Allocator.Temp);
        var fromCharacters = __instance._Query.ToComponentDataArray<FromCharacter>(Allocator.Temp);

        // this variable probably seems weird at first glance.
        // its used to cache some hidden lookups.
        // TODO: handle the caching better. extract things to a service
        var networkIdToEntityMap = NetworkIdLookupMap._NetworkIdToEntityMap;

        for (var i = 0; i < entities.Length; i++)
        {
            var fromSlotIndex = equipItemFromInventoryEvents[i].SlotIndex;
            var inventoryNetworkId = equipItemFromInventoryEvents[i].FromInventory;
            if (!networkIdToEntityMap.TryGetValue(inventoryNetworkId, out var fromInventoryEntity))
            {
                LogUtil.LogWarning("EquipItemFromInventorySystem_OnUpdate_Prefix: Failed to find inventory entity from network id");
                continue;
            }

            bool isValidItemEquip = LoadoutService.IsValidItemEquip(
                character: fromCharacters[i].Character,
                fromInventory: fromInventoryEntity,
                fromSlotIndex: fromSlotIndex,
                isCosmetic: equipItemFromInventoryEvents[i].IsCosmetic
            );

            if (!isValidItemEquip)
            {
                EntityManager.DestroyEntity(entities[i]);
            }
        }
    }

    // EquipItemSystem covers the case of directly trying to equip an item from the player's inventory.
    // "directly" meaning via a hotkey / right clicking the item.
    // It does not cover the case of dragging an item into a designated equip slot.
    [HarmonyPatch(typeof(EquipItemSystem), nameof(EquipItemSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void EquipItemFSystem_OnUpdate_Prefix(EquipItemSystem __instance)
    {
        if (LoadoutService is null)
        {
            return;
        }

        var query0 = __instance.__query_1850505309_0;
        var entities = query0.ToEntityArray(Allocator.Temp);
        var equipItemEvents = query0.ToComponentDataArray<EquipItemEvent>(Allocator.Temp);
        var fromCharacters = query0.ToComponentDataArray<FromCharacter>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var fromSlotIndex = equipItemEvents[i].SlotIndex;

            bool isValidItemEquip = LoadoutService.IsValidItemEquip(
                character: fromCharacters[i].Character,
                fromInventory: fromCharacters[i].Character,
                fromSlotIndex: fromSlotIndex,
                isCosmetic: equipItemEvents[i].IsCosmetic
            );

            if (!isValidItemEquip)
            {
                EntityManager.DestroyEntity(entities[i]);
            }
        }
    }


    [HarmonyPatch(typeof(InventoryUtilitiesServer), nameof(InventoryUtilitiesServer.TryUnEquipItem), new Type[] { typeof(EntityManager), typeof(Entity), typeof(Entity), typeof(Il2CppSystem.Nullable_Unboxed<EntityCommandBuffer>) })]
    [HarmonyPrefix]
    public static bool TryUnEquipItem_Prefix(
        ref bool __result,
        EntityManager entityManager,
        Entity target,
        Entity item,
        Il2CppSystem.Nullable_Unboxed<EntityCommandBuffer> commandBuffer
    )
    {
        //LogUtil.LogDebug("running TryUnEquipItem_Prefix1");
        // This gets called when unequipping an item from the inventory or from a designated slot.
        // But it does NOT get called when unequipping during pvp. Something else cuts it off.
        // Our patch TryUnEquipAndAddItem_Prefix prevents TryUnEquipAndAddItem from running, which presumably is what would call TryUnEquipItem.
        return EXECUTE_ORIGINAL_METHOD;
    }

    [HarmonyPatch(typeof(InventoryUtilitiesServer), nameof(InventoryUtilitiesServer.TryUnEquipItem), new Type[] { typeof(EntityManager), typeof(Entity), typeof(PrefabGUID), typeof(Il2CppSystem.Nullable_Unboxed<EntityCommandBuffer>) })]
    [HarmonyPrefix]
    public static bool TryUnEquipItem_Prefix(
        ref bool __result,
        EntityManager entityManager,
        Entity target,
        PrefabGUID type,
        Il2CppSystem.Nullable_Unboxed<EntityCommandBuffer> commandBuffer
    )
    {
        //LogUtil.LogDebug("running TryUnEquipItem_Prefix2");
        // This doesn't seem to come into play, but leaving it here just in case 
        return EXECUTE_ORIGINAL_METHOD;
    }

    // DropItemSystem covers the case of directly dropping items from a player's own inventory.
    // "directly" meaning via a hotkey.
    // It does not cover dropping items from designated slots or via dragging out of the inventory.
    [HarmonyPatch(typeof(DropItemSystem), nameof(DropItemSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void DropItemSystem_OnUpdate_Prefix(DropItemSystem __instance)
    {
        if (LoadoutService is null)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var dropItemAtSlotEvents = query.ToComponentDataArray<DropItemAtSlotEvent>(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var isValidDrop = LoadoutService.IsValidItemDrop(
                character: fromCharacters[i].Character,
                slotIndex: dropItemAtSlotEvents[i].SlotIndex
            );

            if (!isValidDrop)
            {
                EntityManager.DestroyEntity(entities[i]);
            }
        }
    }


    // todo: disable crafting forbidden items? not necessary, just more user friendly.

    // todo: command to unequip forbidden items from everybody


    //[HarmonyPatch(typeof(DropItemSystem), nameof(DropItemSystem.OnUpdate))]
    //[HarmonyPrefix]
    public static void SomeSystem_OnUpdate_Prefix(DropItemSystem __instance)
    {
        LogUtil.LogInfo("========================================");
        var queryCount = 0;
        foreach (var query in __instance.EntityQueries)
        {
            LogUtil.LogInfo($"query#{queryCount}--------------------------------");
            var entities = query.ToEntityArray(Allocator.Temp);
            for (var i = 0; i < entities.Length; i++)
            {
                DebugUtil.LogComponentTypes(entities[i]);
            }
            queryCount++;
        }
    }

}
