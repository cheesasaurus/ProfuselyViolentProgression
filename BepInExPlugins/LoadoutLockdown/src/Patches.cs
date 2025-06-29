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

    // todo: remove this
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidWeaponEquip))]
    [HarmonyPostfix]
    public static void IsValidWeaponEquip_Postfix(ref bool __result, EntityManager entityManager, EquippableData equippableData, EquipItemEvent equipItem, ServerRootPrefabCollection serverRootPrefabs, Entity character, NativeParallelHashMap<PrefabGUID, ItemData> itemHashLookupMap, int weaponSlots)
    {
        LogUtil.LogInfo($"__result: {__result}");
    }

}