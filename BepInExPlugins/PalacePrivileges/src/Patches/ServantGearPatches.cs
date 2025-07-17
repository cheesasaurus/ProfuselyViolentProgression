using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

// todo: direct equipping from inventory (hot key / right click)
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

    // todo: doesn't run?
    /*
    [HarmonyPatch(typeof(EquipServantItemSystem), nameof(EquipServantItemSystem.OnUpdate))]
    [HarmonyPrefix]
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

}