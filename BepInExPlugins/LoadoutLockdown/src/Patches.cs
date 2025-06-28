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


    // note: the original IsValidWeaponEquip is not just a check. it has side effects: moving the item into an open/junk slot
    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidWeaponEquip))]
    [HarmonyPrefix]
    public static bool IsValidWeaponEquip_Prefix(ref bool __result, EntityManager entityManager, EquippableData equippableData, EquipItemEvent equipItem, ServerRootPrefabCollection serverRootPrefabs, Entity character, NativeParallelHashMap<PrefabGUID, ItemData> itemHashLookupMap, int weaponSlots)
    {
        LogUtil.LogWarning("The thing is happening");

        

        if (InventoryUtilities.TryGetItemAtSlot(EntityManager, character, slotIndex: equipItem.SlotIndex, out InventoryBuffer itemInSlot))
        {
            var itemEntity = itemInSlot.ItemEntity._Entity;
            DebugUtil.LogComponentTypes(itemEntity);
        }


        //DebugUtil.LogComponentTypes(character);
        LogUtil.LogWarning("The thing is happening");
        var prefab = DebugUtil.LookupPrefabName(equippableData.EquipmentSet);
        LogUtil.LogDebug($"prefab: {prefab}");


        //////////////////////////////////////
        /// logic
        ///
        ///

        //__result = true;
        //return false; 

        // if the service isn't ready yet, use the vanilla behaviour
        bool IsServiceReady = false;
        if (!IsServiceReady)
        {
            return true;
        }

        // if it's cosmetic, use the vanilla behaviour
        if (equipItem.IsCosmetic)
        {
            return true;
        }

        bool IsEquipmentForbidden = false;
        if (IsEquipmentForbidden)
        {
            __result = false;
            return false;
        }

        bool IsEquippableWithoutSlot = false;
        if (IsEquippableWithoutSlot)
        {
            __result = true;
            return false;
        }

        bool HasOwnSlot = true;
        if (HasOwnSlot)
        {
            bool IsSlotWasted = false; // todo: service method
            __result = IsSlotWasted;
            // if __result is true, the game will take care of swapping the equipped item into the slot.
            // but only for things that have their own designated slot
            return false;
        }

        bool InValidSlot = true;
        if (InValidSlot)
        {
            __result = true;
            return false;
        }

        // todo: TryFindWastedSlot
        bool HasWastedWeaponSlot = false;
        int WastedWeaponSlotIndex = 0;

        if (HasWastedWeaponSlot)
        {
            // IsValidWeaponEquip has a side effect of swapping the item into a wasted slot,
            // so we mimic that ourselves. But with different logic about what counts as a wasted slot.
            bool IsSlotEmpty = false;
            if (IsSlotEmpty)
            {
                // move item into slot
            }
            else
            {
                // todo: swap item with whatever's in wasted slot
            }
            __result = true;
            return false;
        }

        __result = false;
        return false;
    }

    [HarmonyPatch(typeof(NewWeaponEquipmentRestrictionsUtility), nameof(NewWeaponEquipmentRestrictionsUtility.IsValidWeaponEquip))]
    [HarmonyPostfix]
    public static void IsValidWeaponEquip_Postfix(ref bool __result, EntityManager entityManager, EquippableData equippableData, EquipItemEvent equipItem, ServerRootPrefabCollection serverRootPrefabs, Entity character, NativeParallelHashMap<PrefabGUID, ItemData> itemHashLookupMap, int weaponSlots)
    {
        //LogUtil.LogInfo($"original __result: {__result}");        

        //__result = false;
        LogUtil.LogInfo($"__result: {__result}");
    }

}