using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.LoadoutLockdown;


[HarmonyPatch]
public unsafe class Patches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;
    private static EntityQuery Query;
    private static LoadoutLockdownService LoadoutService => LoadoutLockdownService.Instance;

    private static NativeParallelHashMap<PrefabGUID, ItemData> ItemHashLookupMap => WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>().ItemHashLookupMap; // todo: cache this. should be on a service probably

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
    public static void TryAddItem(ref AddItemSettings addItemSettings, Entity target, InventoryBuffer inventoryItem)
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
    

    // todo: swapping designated slots (e.g. amulet slot) with things in main inventory

    // todo: swapping designated slots (e.g. amulet slot) with external inventories




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
