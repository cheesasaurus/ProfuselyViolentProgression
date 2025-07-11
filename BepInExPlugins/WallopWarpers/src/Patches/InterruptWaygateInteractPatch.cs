using System;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.WallopWarpers.Patches;

[HarmonyPatch]
public static class InterruptWaygateInteractPatch
{
    public static EntityManager EntityManager => WorldUtil.Server.EntityManager;

    [HarmonyPatch(typeof(TeleportationRequestSystem), nameof(TeleportationRequestSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void TeleportationRequestSystem_OnUpdate_Prefix(TeleportationRequestSystem __instance)
    {
        if (Plugin.PvPCombat_AllowWaygateUse.Value)
        {
            return;
        }

        var query = __instance._TeleportRequestQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<TeleportationRequest>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            var entity = entities[i];
            var ev = events[i];
            if (WallopWarpersUtil.IsInPvpCombat(ev.PlayerEntity))
            {
                EntityManager.DestroyEntity(entity);
                WallopWarpersUtil.SendMessagePvPTeleportDisallowed(ev.PlayerEntity);
            }
        }
    }

    [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void AbilityRunScriptsSystem_Prefix(AbilityRunScriptsSystem __instance)
    {
        if (Plugin.PvPCombat_AllowWaygateUse.Value)
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
        if (!EntityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        if (!EntityManager.TryGetComponentData<PrefabGUID>(ev.Ability, out var abilityPrefabGUID))
        {
            return;
        }

        if (WallopWarpersUtil.IsUseWaypointCast(abilityPrefabGUID) && WallopWarpersUtil.IsInPvpCombat(ev.Character))
        {
            try
            {
                WallopWarpersUtil.SendMessagePvPTeleportDisallowed(ev.Character);
                //WallopWarpersUtil.ImpairWaypointUse(ev.Character);
                //WallopWarpersUtil.InterruptCast1(entity, ev); // this doesn't work
                //WallopWarpersUtil.InterruptCast2(entity, ev); // this doesn't work
                //EntityManager.DestroyEntity(entity); // this doesn't work, and causes jank

                WallopWarpersUtil.InterruptCast3(entity, ev);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }
    }

}