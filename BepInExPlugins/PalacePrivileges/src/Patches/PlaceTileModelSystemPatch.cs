using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

// CastleBuilding.CharacterMenuOpenedSystem_Server


[HarmonyPatch]
public unsafe class PlaceTileModelSystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void SomePatchThing(PlaceTileModelSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        ProcessEvents_BuildTile(__instance._BuildTileQuery, ref networkIdToEntityMap);
        ProcessEvents_StartEdit(__instance._StartEditQuery, ref networkIdToEntityMap);
        ProcessEvents_CancelEdit(__instance._CancelEditQuery, ref networkIdToEntityMap);
        ProcessEvents_MoveTile(__instance._MoveTileQuery, ref networkIdToEntityMap);
        ProcessEvents_DismantleTile(__instance._DismantleTileQuery, ref networkIdToEntityMap);
        ProcessEvents_RepairTile(__instance._RepairTileQuery, ref networkIdToEntityMap);
        ProcessEvents_BuildWallpaper(__instance._BuildWallpaperQuery, ref networkIdToEntityMap);
        ProcessEvents_SetVariation(__instance._SetVariationQuery, ref networkIdToEntityMap);
        ProcessEvents_AbilityCastFinished(__instance._AbilityCastFinishedQuery, ref networkIdToEntityMap);
    }

    private static void ProcessEvents_BuildTile(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<BuildTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = tmEvents[i];
            LogUtil.LogDebug($"BuildTile: {DebugUtil.LookupPrefabName(ev.PrefabGuid)}");

            if (!Core.BuildingService.IsStructureRestrictedToOwnedArea(ev.PrefabGuid))
            {
                continue;
            }

            // todo: implement
            // need to relate to the castle somehow
            

            // todo: maybe something with the static GetPlacementResult class.
            // Entity `buildingInCastleTerritory`
            // Entity `castleHeartForArea`
            // `TerrainChunk`
            // `TerrainChunkLookup.TryGetChunk`
            // `TerrainChunkLookup.TryGetChunkMetadataEntity`
            // `TerrainConstants`


        }
    }

    private static void ProcessEvents_StartEdit(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<StartEditTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            if (!TryGetDataForExistingTileModel(tmEvents[i].Target, ref networkIdToEntityMap, out var tileModel, out var castleHeartConnection))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_BuildStartEdit(character, tileModel, castleHeartConnection);
            EnforceRuling(entities[i], character, ruling);
        }
    }

    private static void ProcessEvents_CancelEdit(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        // no restrictions on canceling editing

        /*
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<CancelEditTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            LogUtil.LogDebug("CancelEdit");
        }
        */
    }

    private static void ProcessEvents_MoveTile(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        // Restrictions for this are covered by StartEdit and BuildTile.

        /*
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<MoveTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            LogUtil.LogDebug("MoveTile");
        }
        */
    }

    private static void ProcessEvents_DismantleTile(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<DismantleTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            if (!TryGetDataForExistingTileModel(tmEvents[i].Target, ref networkIdToEntityMap, out var tileModel, out var castleHeartConnection))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_BuildDismantle(character, tileModel, castleHeartConnection);
            EnforceRuling(entities[i], character, ruling);
        }
    }

    private static void ProcessEvents_RepairTile(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        // no restrictions on repairing

        /*
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<RepairTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            LogUtil.LogDebug("RepairTile");
        }
        */
    }

    private static void ProcessEvents_BuildWallpaper(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<BuildWallpaperEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            if (!TryGetDataForExistingTileModel(tmEvents[i].Target, ref networkIdToEntityMap, out var tileModel, out var castleHeartConnection))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_BuildWallpaper(character, tileModel, castleHeartConnection);
            EnforceRuling(entities[i], character, ruling);
        }
    }

    private static void ProcessEvents_SetVariation(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<SetTileModelVariationEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            LogUtil.LogDebug("SetVariation");
            if (!TryGetDataForExistingTileModel(tmEvents[i].Target, ref networkIdToEntityMap, out var tileModel, out var castleHeartConnection))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_BuildSetVariation(character, tileModel, castleHeartConnection);
            EnforceRuling(entities[i], character, ruling);
        }
    }

    private static void ProcessEvents_ConnectionChanged(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        // nothing to do... yet

        /*
        var entities = query.ToEntityArray(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<UserConnectionChangedEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            
        }
        */
    }

    private static void ProcessEvents_AbilityCastFinished(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        // nothing to do... yet

        /*
        var entities = query.ToEntityArray(Allocator.Temp);
        var abilityEvents = query.ToComponentDataArray<AbilityPreCastFinishedEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            DebugUtil.LogPrefabGuid(abilityEvents[i].Ability);
        }
        */
    }

    private static bool TryGetDataForExistingTileModel(
        NetworkId tileModelNetworkId,
        ref NetworkIdLookupMap networkIdToEntityMap,
        out Entity tileModelEntity,
        out CastleHeartConnection castleHeartConnection
    )
    {
        castleHeartConnection = default;

        if (!networkIdToEntityMap.TryGetValue(tileModelNetworkId, out tileModelEntity))
        {
            return false;
        }

        if (!_entityManager.TryGetComponentData<CastleHeartConnection>(tileModelEntity, out castleHeartConnection))
        {
            return false;
        }

        return true;
    }
    
    private static void EnforceRuling(Entity eventEntity, Entity character, CastleActionRuling ruling)
    {
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
            _entityManager.DestroyEntity(eventEntity);
        }
    }

}