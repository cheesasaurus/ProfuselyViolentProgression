using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

// todo: dragging into servant slot from own equipment slot (not hotbar)
// todo: dragging into own equipment slot (not hotbar) from servant slot

[HarmonyPatch]
public unsafe class ServantGearPatches
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(EquipServantItemFromInventorySystem), nameof(EquipServantItemFromInventorySystem.OnUpdate))]
    [HarmonyPrefix]
    public static void EquipServantItemFromInventorySystem_OnUpdate_Prefix(EquipServantItemFromInventorySystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._Query;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var equipEvents = query.ToComponentDataArray<EquipServantItemFromInventoryEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = equipEvents[i];

            if (!networkIdToEntityMap.TryGetValue(ev.ToEntity, out var servant))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_ServantGearChange(character, servant);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }

        }
    }

    [HarmonyPatch(typeof(UnEquipServantItemSystem), nameof(UnEquipServantItemSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void UnEquipServantItemSystem_OnUpdate_Prefix(UnEquipServantItemSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._Query;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var equipEvents = query.ToComponentDataArray<UnequipServantItemEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = equipEvents[i];

            if (!networkIdToEntityMap.TryGetValue(ev.FromEntity, out var servant))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_ServantGearChange(character, servant);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                // todo: uncomment
                //_entityManager.DestroyEntity(entities[i]);
            }

        }
    }

    // todo: doesn't run?
    /*
    [HarmonyPatch(typeof(EquipServantItemSystem), nameof(EquipServantItemSystem.OnUpdate))]
    [HarmonyPrefix]
    [EcsSystemUpdatePrefix(typeof(EquipServantItemSystem))]
    public static void EquipServantItemSystem_OnUpdate_Prefix(EquipServantItemSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var equipEvents = query.ToComponentDataArray<EquipServantItemEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            LogUtil.LogDebug("Doing the thing");
            var ev = equipEvents[i];

            if (!networkIdToEntityMap.TryGetValue(ev.ToEntity, out var servant))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_ServantGearChange(character, servant);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }

        }
    }
    */

    // this does not fire for servants.
    // EquipItemSystem covers the case of directly trying to equip an item from a player's inventory.
    // "directly" meaning via a hotkey / right clicking the item.
    // It does not cover the case of dragging an item into a designated equip slot.
    //[HarmonyPatch(typeof(EquipItemSystem), nameof(EquipItemSystem.OnUpdate))]
    //[HarmonyPrefix]
    public static void EquipItemSystem_OnUpdate_Prefix(EquipItemSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance.__query_1850505309_0;
        var entities = query.ToEntityArray(Allocator.Temp);
        var equipItemEvents = query.ToComponentDataArray<EquipItemEvent>(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var character = fromCharacters[i].Character;
            var fromSlotIndex = equipItemEvents[i].SlotIndex;

            //DebugUtil.LogComponentTypes(entities[0]);
        }
    }

    // this does not fire for servants
    //[HarmonyPatch(typeof(EquipItemFromInventorySystem), nameof(EquipItemFromInventorySystem.OnUpdate))]
    //[HarmonyPrefix]
    public static void EquipItemFromInventorySystem_OnUpdate_Prefix(EquipItemFromInventorySystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var entities = __instance._Query.ToEntityArray(Allocator.Temp);
        var equipItemFromInventoryEvents = __instance._Query.ToComponentDataArray<EquipItemFromInventoryEvent>(Allocator.Temp);
        var fromCharacters = __instance._Query.ToComponentDataArray<FromCharacter>(Allocator.Temp);

        // this variable probably seems weird at first glance.
        // its used to cache some hidden lookups.
        // TODO: handle the caching better. extract things to a service
        //var networkIdToEntityMap = NetworkIdLookupMap._NetworkIdToEntityMap;

        for (var i = 0; i < entities.Length; i++)
        {
            var character = fromCharacters[i].Character;
            var fromSlotIndex = equipItemFromInventoryEvents[i].SlotIndex;
            var inventoryNetworkId = equipItemFromInventoryEvents[i].FromInventory;

            DebugUtil.LogComponentTypes(entities[i]);
        }
    }

}