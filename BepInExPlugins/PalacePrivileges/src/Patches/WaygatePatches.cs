using System;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public static class InterruptWaygateInteractPatch
{
    private static EntityManager _entityManager => WorldUtil.Server.EntityManager;

    [HarmonyPatch(typeof(TeleportationRequestSystem), nameof(TeleportationRequestSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void TeleportationRequestSystem_OnUpdate_Prefix(TeleportationRequestSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._TeleportRequestQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<TeleportationRequest>(Allocator.Temp);
        
        for (var i = 0; i < entities.Length; i++)
        {
            var ev = events[i];

            if (!_entityManager.HasComponent<CastleWaypoint>(ev.ToTarget))
            {
                continue;
            }

            var character = ev.PlayerEntity;
            var ruling = Core.RestrictionService.ValidateAction_WaygateIn(character, ev.ToTarget);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }
        }
    }

    [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void AbilityRunScriptsSystem_Prefix(AbilityRunScriptsSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }
        ProcessQuery_OnCastStarted(__instance);
    }

    private static void ProcessQuery_OnCastStarted(AbilityRunScriptsSystem __instance)
    {
        var query = __instance._OnCastStartedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            OnCastStarted(entities[i], events[i]);
        }
    }

    private static void OnCastStarted(Entity entity, AbilityCastStartedEvent ev)
    {
        if (!_entityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        if (!_entityManager.TryGetComponentData<PrefabGUID>(ev.Ability, out var abilityPrefabGUID))
        {
            return;
        }

        bool isUsingCastleWaygate = abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseWaypoint_Castle_Cast);
        if (!isUsingCastleWaygate)
        {
            return;
        }

        if (!_entityManager.TryGetComponentData<AbilityTarget>(ev.Ability, out var abilityTarget))
        {
            return;
        }

        var ruling = Core.RestrictionService.ValidateAction_WaygateOut(ev.Character, abilityTarget.Target._Entity);
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(ev.Character, ref ruling);
            Core.ServerGameManager.InterruptCast(ev.Character);
        }        
    }

}