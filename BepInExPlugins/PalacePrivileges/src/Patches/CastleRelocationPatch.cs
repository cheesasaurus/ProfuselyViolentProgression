using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM.CastleBuilding;
using ProjectM.CastleBuilding.Rebuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class CastleRelocationPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;
    
    private static EntityQuery _queryConnectCastleForRelocation = _entityManager.CreateEntityQuery(new EntityQueryDesc()
    {
        All = new ComponentType[] {
            ComponentType.ReadOnly<FromCharacter>(),
            ComponentType.ReadOnly<CastleRebuildConnectEvent>(),
        },
    });

    [EcsSystemUpdatePrefix(typeof(CastleRebuildRegistryServerEventSystem))]
    public static void CastleRebuildRegistryServerEventSystem_OnUpdate_Prefix()
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = _queryConnectCastleForRelocation;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var connectCastleEvents = query.ToComponentDataArray<CastleRebuildConnectEvent>(Allocator.Temp);

        var mapZoneCollection = Core.SingletonService.FetchMapZoneCollection();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = connectCastleEvents[i];
            var character = fromCharacters[i].Character;
            var formerAddress = ev.SourceTerritory;

            if (!mapZoneCollection.MapZoneLookup.TryGetValue(formerAddress, out var mapZoneData))
            {
                continue;
            }

            if (!_entityManager.TryGetComponentData<CastleTerritory>(mapZoneData.ZoneEntity, out var castleTerritory))
            {
                continue;
            }

            var ruling = Core.RestrictionService.ValidateAction_CastleHeartRelocate(character, castleTerritory.CastleHeart);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }
        }
        
    }

}