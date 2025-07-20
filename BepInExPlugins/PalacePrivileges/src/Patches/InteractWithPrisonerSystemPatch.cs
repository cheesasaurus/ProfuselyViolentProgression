using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;


[HarmonyPatch]
public unsafe class InteractWithPrisonerSystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(InteractWithPrisonerSystem), nameof(InteractWithPrisonerSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void SomePatchThing(InteractWithPrisonerSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var prisonerEvents = query.ToComponentDataArray<InteractWithPrisonerEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = prisonerEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.Prison, out var prison))
            {
                continue;
            }

            switch (ev.PrisonInteraction)
            {
                default:
                case EventHelper.PrisonInteraction.Imprison:
                    // allowed; do nothing
                    break;

                case EventHelper.PrisonInteraction.Charm:
                    HandleCharm(entities[i], character, prison);
                    break;

                case EventHelper.PrisonInteraction.Kill:
                    HandleKill(entities[i], character, prison);
                    break;
            }
        }
    }

    private static void HandleCharm(Entity eventEntity, Entity character, Entity prison)
    {
        var ruling = Core.RestrictionService.ValidateAction_PrisonerSubdue(character, prison);
        EnforceRuling(eventEntity, character, ref ruling);
    }

    private static void HandleKill(Entity eventEntity, Entity character, Entity prison)
    {
        var ruling = Core.RestrictionService.ValidateAction_PrisonerKill(character, prison);
        EnforceRuling(eventEntity, character, ref ruling);
    }
    
    private static void EnforceRuling(Entity eventEntity, Entity character, ref CastleActionRuling ruling)
    {
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
            _entityManager.DestroyEntity(eventEntity);
        }
    }

}