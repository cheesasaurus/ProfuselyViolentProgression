using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM.Contest.Arena;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;


[HarmonyPatch]
public unsafe class ArenaZonePaintingPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;

    private static EntityQuery _queryArenaBlockEvents = _entityManager.CreateEntityQuery(new EntityQueryDesc()
    {
        All = new ComponentType[] {
            ComponentType.ReadOnly<FromCharacter>(),
            ComponentType.ReadOnly<CastleArenaBlockOperationEvent>(),
        },
    });

    [EcsSystemUpdatePrefix(typeof(CastleArenaBlockOperationEventSystem))]
    public static void EnforceArenaZonePaintingRestrictions()
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = _queryArenaBlockEvents;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var arenaBlockEvents = query.ToComponentDataArray<CastleArenaBlockOperationEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = arenaBlockEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.CastleArenaStation, out var arenaStation))
            {
                continue;
            }

            switch (ev.Operation)
            {
                case CastleArenaBlockOperationEvent.Type.Add:
                case CastleArenaBlockOperationEvent.Type.Remove:
                case CastleArenaBlockOperationEvent.Type.Clear:
                    var ruling = Core.RestrictionService.ValidateAction_ArenaPaintZone(character, arenaStation);
                    if (!ruling.IsAllowed)
                    {
                        Core.NotificationService.NotifyActionDenied(character, ref ruling);
                        _entityManager.DestroyEntity(entities[i]);
                    }
                    break;
            }
        }
    }

}