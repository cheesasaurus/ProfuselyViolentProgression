using System;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public static class AbilityCastPatches
{
    private static EntityManager _entityManager => WorldUtil.Server.EntityManager;

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
        if (!_entityManager.TryGetComponentData<AbilityTarget>(ev.Ability, out var abilityTarget))
        {
            return;
        }
        if (abilityTarget.GetTargetType is not AbilityTarget.Type.InteractTarget)
        {
            LogUtil.LogWarning("not interact target"); // todo: remove log
            return;
        }
        if (!_entityManager.TryGetComponentData<PrefabGUID>(ev.Ability, out var abilityPrefabGUID))
        {
            return;
        }
        if (!_entityManager.TryGetComponentData<PrefabGUID>(abilityTarget.Target._Entity, out var targetPrefabGUID))
        {
            return;
        }

        DebugUtil.LogPrefabGuid(ev.Ability); // todo: remove log
        LogUtil.LogDebug($"target: {DebugUtil.LookupPrefabName(targetPrefabGUID)}");
        LogUtil.LogDebug("-------");

        // waygate
        if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseWaypoint_Castle_Cast))
        {
            Handle_UsingCastleWaypoint(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }

        // small teleporters. the purple teleporter uses the red ability for some reason
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_LocalCastleTeleport_Red_Cast))
        {
            Handle_UsingSmallTeleporter(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_LocalCastleTeleport_Yellow_Cast))
        {
            Handle_UsingSmallTeleporter(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_LocalCastleTeleport_Blue_Cast))
        {
            Handle_UsingSmallTeleporter(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }

        // others
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_OpenContainer_Cast))
        {
            Handle_OpeningContainer(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseMusicPlayerStation_Cast))
        {
            Handle_UsingMusicbox(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseResearchstation_Cast))
        {
            Handle_UsingResearchStation(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseInventoryRouteStation_Cast))
        {
            Handle_UsingRedistributionEngine(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_ArenaStation_Cast))
        {
            Handle_UsingArenaStation(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_Throne_Cast))
        {
            Handle_UsingThrone(ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
    }

    private static void EnforceRuling(Entity character, CastleActionRuling ruling)
    {
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
            Core.ServerGameManager.InterruptCast(character);
        }
    }

    private static void Handle_UsingCastleWaypoint(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_WaygateOut(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ev.Character, ruling);
    }

    private static void Handle_UsingSmallTeleporter(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        // todo: implement
    }

    private static void Handle_UsingThrone(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessThrone(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ev.Character, ruling);
    }

    private static void Handle_UsingArenaStation(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessArenaStation(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ev.Character, ruling);
    }

    private static void Handle_UsingRedistributionEngine(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessRedistributionEngine(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ev.Character, ruling);
    }

    private static void Handle_UsingMusicbox(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessMusicbox(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ev.Character, ruling);
    }

    private static void Handle_OpeningContainer(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        if (targetPrefabGUID.Equals(PrefabGuids.TM_Stash_Chest_SafetyBox))
        {
            var ruling = Core.RestrictionService.ValidateAction_AccessLockbox(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ev.Character, ruling);
        }
    }

    private static void Handle_UsingResearchStation(AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        // todo: implement
        
    }

}