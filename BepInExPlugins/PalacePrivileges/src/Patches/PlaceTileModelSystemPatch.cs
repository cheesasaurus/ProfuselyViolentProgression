using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
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
            // todo: implement
        }
    }

    private static void ProcessEvents_StartEdit(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<StartEditTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

    private static void ProcessEvents_CancelEdit(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<CancelEditTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

    private static void ProcessEvents_MoveTile(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<MoveTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

    private static void ProcessEvents_DismantleTile(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<DismantleTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

    private static void ProcessEvents_RepairTile(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<RepairTileModelEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

    private static void ProcessEvents_BuildWallpaper(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<BuildWallpaperEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

    private static void ProcessEvents_SetVariation(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<SetTileModelVariationEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }
    
    private static void ProcessEvents_ConnectionChanged(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var tmEvents = query.ToComponentDataArray<UserConnectionChangedEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

    private static void ProcessEvents_AbilityCastFinished(EntityQuery query, ref NetworkIdLookupMap networkIdToEntityMap)
    {
        var entities = query.ToEntityArray(Allocator.Temp);
        var abilityEvents = query.ToComponentDataArray<AbilityPreCastFinishedEvent>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            // todo: implement
        }
    }

}