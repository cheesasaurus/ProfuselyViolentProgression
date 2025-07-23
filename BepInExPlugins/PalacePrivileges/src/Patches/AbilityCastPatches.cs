using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

// todo: there is a lot of property drilling here.
// consider a nested class to hold the data needed,
// with its own methods like Handle_UsingArenaStation, etc.
// Which is basically a command/task/job... at that point I might as well read up on Unity jobs.
// https://docs.unity3d.com/Manual/job-system.html

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
        var serverGameManager = Core.FetchServerGameManager();
        ProcessQuery_OnCastStarted(__instance, ref serverGameManager);
    }

    private static void ProcessQuery_OnCastStarted(AbilityRunScriptsSystem __instance, ref ServerGameManager serverGameManager)
    {
        var query = __instance._OnCastStartedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            OnCastStarted(ref serverGameManager, entities[i], events[i]);
        }
    }

    private static void OnCastStarted(ref ServerGameManager serverGameManager, Entity entity, AbilityCastStartedEvent ev)
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

        // waygate
        if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseWaypoint_Castle_Cast))
        {
            Handle_UsingCastleWaypoint(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }

        // small teleporters. the purple teleporter uses the red ability for some reason
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_LocalCastleTeleport_Red_Cast))
        {
            Handle_UsingSmallTeleporter(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_LocalCastleTeleport_Yellow_Cast))
        {
            Handle_UsingSmallTeleporter(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_LocalCastleTeleport_Blue_Cast))
        {
            Handle_UsingSmallTeleporter(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }

        // others
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_OpenContainer_Cast))
        {
            Handle_OpeningContainer(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseMusicPlayerStation_Cast))
        {
            Handle_UsingMusicbox(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseResearchstation_Cast))
        {
            Handle_UsingResearchStation(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_UseInventoryRouteStation_Cast))
        {
            Handle_UsingRedistributionEngine(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_ArenaStation_Cast))
        {
            Handle_UsingArenaStation(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
        else if (abilityPrefabGUID.Equals(PrefabGuids.AB_Interact_Throne_Cast))
        {
            Handle_UsingThrone(ref serverGameManager, ev, abilityTarget, abilityPrefabGUID, targetPrefabGUID);
        }
    }

    private static void EnforceRuling(ref ServerGameManager serverGameManager, Entity character, ref CastleActionRuling ruling)
    {
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
            serverGameManager.InterruptCast(character);
        }
    }

    private static void Handle_UsingCastleWaypoint(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_WaygateOut(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
    }

    private static void Handle_UsingSmallTeleporter(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        if (targetPrefabGUID.Equals(PrefabGuids.TM_Castle_LocalTeleporter_Red))
        {
            var ruling = Core.RestrictionService.ValidateAction_UseTeleporterRed(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
        else if (targetPrefabGUID.Equals(PrefabGuids.TM_Castle_LocalTeleporter_Yellow))
        {
            var ruling = Core.RestrictionService.ValidateAction_UseTeleporterYellow(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
        else if (targetPrefabGUID.Equals(PrefabGuids.TM_Castle_LocalTeleporter_Purple))
        {
            var ruling = Core.RestrictionService.ValidateAction_UseTeleporterPurple(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
        else if (targetPrefabGUID.Equals(PrefabGuids.TM_Castle_LocalTeleporter_Blue))
        {
            var ruling = Core.RestrictionService.ValidateAction_UseTeleporterBlue(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
    }

    private static void Handle_UsingThrone(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessThrone(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
    }

    private static void Handle_UsingArenaStation(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessArenaStation(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
    }

    private static void Handle_UsingRedistributionEngine(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessRedistributionEngine(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
    }

    private static void Handle_UsingMusicbox(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_AccessMusicbox(ev.Character, abilityTarget.Target._Entity);
        EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
    }

    private static void Handle_OpeningContainer(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        if (targetPrefabGUID.Equals(PrefabGuids.TM_Stash_Chest_SafetyBox))
        {
            var ruling = Core.RestrictionService.ValidateAction_AccessLockbox(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
    }

    private static void Handle_UsingResearchStation(ref ServerGameManager serverGameManager, AbilityCastStartedEvent ev, AbilityTarget abilityTarget, PrefabGUID abilityPrefabGUID, PrefabGUID targetPrefabGUID)
    {
        if (targetPrefabGUID.Equals(PrefabGuids.TM_ResearchStation_T01))
        {
            var ruling = Core.RestrictionService.ValidateAction_AccessResearchDeskT1(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
        else if (targetPrefabGUID.Equals(PrefabGuids.TM_ResearchStation_T02))
        {
            var ruling = Core.RestrictionService.ValidateAction_AccessResearchDeskT2(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
        else if (targetPrefabGUID.Equals(PrefabGuids.TM_ResearchStation_T03))
        {
            var ruling = Core.RestrictionService.ValidateAction_AccessResearchDeskT3(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
        else if (targetPrefabGUID.Equals(PrefabGuids.TM_StygianAltar_Passive_T01))
        {
            var ruling = Core.RestrictionService.ValidateAction_AccessStygianAltar(ev.Character, abilityTarget.Target._Entity);
            EnforceRuling(ref serverGameManager, ev.Character, ref ruling);
        }
    }

}