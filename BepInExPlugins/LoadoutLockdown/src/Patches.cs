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

    private const bool SKIP_ORIGINAL_METHOD = false;
    private const bool EXECUTE_ORIGINAL_METHOD = true;


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

        if (LoadoutService.HasOwnSlot(itemEntity))
        {
            __result = !isInPvPCombat
                || LoadoutService.CanMenuSwapIntoFilledSlotDuringPVP(itemEntity)
                || LoadoutService.IsOwnSlotWasted(character, itemEntity);
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

    // item "Transferring" is internally called by IsValidItemMove
    // it gets called twice per potential move: once each for the orderings of slotA and slotB
    // i.e. the second time it is called, the former slotA will appear as slotB, and the former slotB will appear as slotA.
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidTransfer))]
    [HarmonyPrefix]
    public static unsafe bool IsValidTransfer_Prefix(
        ref bool __result,
        EntityManager entityManager,
        NativeParallelHashMap<PrefabGUID, ItemData> itemDataMap,
        int slotA,
        int slotB,
        Entity entityA,
        Entity entityB,
        bool isInCombat,
        int weaponSlots,
        bool allowIfEmpty = false
    )
    {
        LogUtil.LogDebug("doing IsValidTransfer");

        LogUtil.LogInfo($"slotA: {slotA}, slotB: {slotB}");

        //DebugUtil.LogComponentTypes(entityA);

        /////////////////////////////////////////////////////////

        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }



        // todo: logic

        return EXECUTE_ORIGINAL_METHOD;
    }

    // todo: remove this
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidTransfer))]
    [HarmonyPostfix]
    public static unsafe void IsValidItemTransfer_Postfix(ref bool __result)
    {
        LogUtil.LogDebug($"IsValidTransfer  __result: {__result}");
    }


    // item "Moving" is between different inventories. or the same inventory.
    // IsValidItemMove seems to internally call IsValidItemTransfer.
    // Neither seems to be called when moving stuff in/out of a designated slot (e.g. cape, armor piece)
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
        LogUtil.LogDebug("doing IsValidItemMove");

        //__result = true;
        //return SKIP_ORIGINAL_METHOD;


        /////////////////////////////////////////////////////////

        if (LoadoutService is null)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        bool fromWeaponSlot = false;
        bool toWeaponSlot = false;
        if (!fromWeaponSlot && !toWeaponSlot)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        // todo: logic

        return EXECUTE_ORIGINAL_METHOD;
    }


    // todo: remove this
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidItemMove))]
    [HarmonyPostfix]
    public static unsafe void IsValidItemMove_Postfix(ref bool __result)
    {
        LogUtil.LogDebug($"IsValidItemMove  __result: {__result}");
    }


    // todo: picking up items off ground should not equip forbidden things
    
    // todo: swapping designated slots (e.g. amulet slot) with things in main inventory

    // todo: swapping designated slots (e.g. amulet slot) with external inventories

}