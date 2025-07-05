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
    // We do not call IsValidItemTransfer at all, unless falling back to the original behaviour.
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

        var ruling = LoadoutService.ValidateItemMove(moveEvent, toInventory, fromInventory);
        if (ruling.ShouldUnEquipItemBeforeMoving)
        {
            InventoryUtilitiesServer.TryUnEquipItem(EntityManager, ruling.ItemToUnEquip.Character, ruling.ItemToUnEquip.Item);
        }
        __result = ruling.IsAllowed;
        return SKIP_ORIGINAL_METHOD;
    }

    // IsValidItemDrop never seems to be called.
    // I'm guessing WeaponSlots was a half-cooked feature that got cut,
    // hence not appearing in any game menu or server settings documentation.
    // Something else prevents hotbar items being dropped in combat.
    //
    // We add our own processing of item drop events in the system update loop,
    // And therefore completely disable this.
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
        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }
        __result = true;
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

        if (!LoadoutService.IsInRestrictiveCombat(target) || LoadoutService.CanDirectlyMoveOutOfSlotDuringPVP(item))
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
    public static void EquipItemSystem_OnUpdate_Prefix(EquipItemSystem __instance)
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

    // TryUnEquipItem gets called when unequipping an item from the inventory or from a designated slot.
    //
    // But it does NOT get called when unequipping from a designated slot during pvp. Something else cuts it off.
    // Our patch TryUnEquipAndAddItem_Prefix prevents TryUnEquipAndAddItem from running, which presumably is what would call TryUnEquipItem.
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

        // TryUnEquipAndAddItem_Prefix already handles unequipping items from designated slots.
        //
        // The only other case this would cover is unequipping items from the hotbar/inventory.
        // Switching to unarmed should always be allowed, so there is nothing to change here.
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
    // This is DropItemAtSlotEvent.
    //
    // It also covers the case of dropping items from a designated slot. (both directly, and via drag-n-drop)
    // This is DropEquippedItemEvent.
    //
    // It does not cover dragging items out of the inventory.
    //
    // Note that there is some other check besides NewWeaponEquipmentRestrictionsUtility.IsValidItemDrop,
    // which prevents dropping items from the hotbar during pvp.
    [HarmonyPatch(typeof(DropItemSystem), nameof(DropItemSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void DropItemSystem_OnUpdate_Prefix(DropItemSystem __instance)
    {
        if (LoadoutService is null)
        {
            return;
        }

        // DropItemAtSlotEvent (from inventory)
        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var dropItemAtSlotEvents = query.ToComponentDataArray<DropItemAtSlotEvent>(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        for (var i = 0; i < entities.Length; i++)
        {
            var isValidDrop = LoadoutService.IsValidItemDrop(
                character: fromCharacters[i].Character,
                fromInventory: fromCharacters[i].Character,
                slotIndex: dropItemAtSlotEvents[i].SlotIndex
            );

            if (!isValidDrop)
            {
                EntityManager.DestroyEntity(entities[i]);
            }
        }

        // DropEquippedItemEvent (from designated slot)
        var query2 = __instance._EventQuery2;
        var entities2 = query2.ToEntityArray(Allocator.Temp);
        var dropEquippedItemEvents = query2.ToComponentDataArray<DropEquippedItemEvent>(Allocator.Temp);
        var fromCharacters2 = query2.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        for (var i = 0; i < entities2.Length; i++)
        {
            var isValidDrop = LoadoutService.IsValidItemDropFromDedicatedSlot(
                character: fromCharacters2[i].Character,
                equipmentType: dropEquippedItemEvents[i].EquipmentType
            );

            if (!isValidDrop)
            {
                EntityManager.DestroyEntity(entities2[i]);
            }
        }

    }

    // DropInventoryItemSystem covers the case of drag-n-dropping items out of inventories.
    // It can include a player's own inventory, as well as external inventories.
    //
    // Note that there is some other check besides NewWeaponEquipmentRestrictionsUtility.IsValidItemDrop,
    // which prevents dropping items from the hotbar during pvp.
    [HarmonyPatch(typeof(DropInventoryItemSystem), nameof(DropInventoryItemSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void DropInventoryItemSystem_OnUpdate_Prefix(DropInventoryItemSystem __instance)
    {
        if (LoadoutService is null)
        {
            return;
        }

        // todo: cache query
        var query = EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<DropInventoryItemEvent>(),
                ComponentType.ReadOnly<FromCharacter>(),
            },
        });

        var entities = query.ToEntityArray(Allocator.Temp);
        var dropInventoryItemEvents = query.ToComponentDataArray<DropInventoryItemEvent>(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);

        // this variable probably seems weird at first glance.
        // its used to cache some hidden lookups.
        // TODO: handle the caching better. extract things to a service
        var networkIdToEntityMap = NetworkIdLookupMap._NetworkIdToEntityMap;

        for (var i = 0; i < entities.Length; i++)
        {
            var inventoryNetworkId = dropInventoryItemEvents[i].Inventory;
            if (!networkIdToEntityMap.TryGetValue(inventoryNetworkId, out var fromInventoryEntity))
            {
                LogUtil.LogWarning("DropInventoryItemSystem_OnUpdate_Prefix: Failed to find inventory entity from network id");
                continue;
            }

            var isValidDrop = LoadoutService.IsValidItemDrop(
                character: fromCharacters[i].Character,
                fromInventory: fromInventoryEntity,
                slotIndex: dropInventoryItemEvents[i].SlotIndex
            );

            if (!isValidDrop)
            {
                EntityManager.DestroyEntity(entities[i]);
            }
        }
    }


    // todo: disable crafting forbidden items? not necessary, just more user friendly.
    // Would need need to investigate the crafting / recipe implementation details.
    // In particular, need to map recipe prefabs to actual item prefabs.
    // and the item prefabs can be used to get the EquipmentType / WeaponType from item entities.
    // Item entities gotten from ItemData, looked up from ItemHashLookupMap.


    //[HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
    //[HarmonyPrefix]
    //public static void SomeSystem_OnUpdate_Prefix(StartCraftingSystem __instance)
    //{
    //    DebugUtil.LogComponentTypesFromQueries(__instance);
    //}

}
