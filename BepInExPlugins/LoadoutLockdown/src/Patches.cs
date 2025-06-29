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
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidWeaponEquip))]
    [HarmonyPrefix]
    public static bool IsValidWeaponEquip_Prefix(ref bool __result, EntityManager entityManager, EquippableData equippableData, EquipItemEvent equipItem, ServerRootPrefabCollection serverRootPrefabs, Entity character, NativeParallelHashMap<PrefabGUID, ItemData> itemHashLookupMap, int weaponSlots)
    {
        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        if (equipItem.IsCosmetic)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        if (!InventoryUtilities.TryGetItemAtSlot(EntityManager, character, slotIndex: equipItem.SlotIndex, out InventoryBuffer itemInSlot))
        {
            return EXECUTE_ORIGINAL_METHOD;
        }
        var itemEntity = itemInSlot.ItemEntity._Entity;

        if (LoadoutService.IsEquipmentForbidden(itemEntity))
        {
            LoadoutService.SendMessageEquipmentForbidden(character);
            __result = false;
            return SKIP_ORIGINAL_METHOD;
        }

        if (LoadoutService.IsEquippableWithoutSlot(itemEntity))
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        bool isInPvPCombat = NewWeaponEquipmentRestrictionsUtility.IsInPvPCombat(EntityManager, serverRootPrefabs, character);

        if (LoadoutService.HasDesignatedSlot(itemEntity))
        {
            __result = !isInPvPCombat
                || LoadoutService.CanMenuSwapIntoFilledSlotDuringPVP(itemEntity)
                || LoadoutService.IsDesignatedSlotWasted(character, itemEntity);

            if (__result is false)
            {
                LoadoutService.SendMessageCannotMenuSwapDuringPVP(character);
            }
            // if __result is true, the game will take care of swapping the equipped item into the slot.
            // but only for things that have their own designated slot
            return SKIP_ORIGINAL_METHOD;
        }

        if (LoadoutService.IsValidWeaponSlot(equipItem.SlotIndex))
        {
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        // IsValidWeaponEquip has a side effect of swapping the item into a wasted slot,
        // so we mimic that ourselves. But with different rules about what counts as a wasted slot.
        if (LoadoutService.TryFindWastedWeaponSlot(character, out var wastedSlotIndex))
        {
            LoadoutService.SwapItemsInSameInventory(character, equipItem.SlotIndex, wastedSlotIndex);
            __result = true;
            return SKIP_ORIGINAL_METHOD;
        }

        __result = false;
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

    // IsValidItemDrop never seems to be called.
    // I'm guessing WeaponSlots was a half-cooked feature that got cut,
    // hence not appearing in any game menu or server settings documentation.
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


    // todo: overloads
    public static bool TryUnEquipItem_Prefix()
    {
        return EXECUTE_ORIGINAL_METHOD;
    }

    // todo: set server setting for WeaponSlots to match the mod's settings.
    // and put it back when the mod unloads.

    // todo: dropping items from designated slots

    // todo: disable crafting forbidden items? not necessary, just more user friendly.

    // todo: command to unequip forbidden items from everybody




    //[HarmonyPatch(typeof(EquipItemFromInventorySystem), nameof(EquipItemFromInventorySystem.OnUpdate))]
    //[HarmonyPrefix]
    public static void EquipItemFromInventorySystem_OnUpdate_Prefix(EquipItemFromInventorySystem __instance)
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
