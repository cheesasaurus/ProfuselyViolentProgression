using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

/// <summary>
/// Knows things about seeds and saplings
/// </summary>
public class GardenService
{
    private EntityManager _entityManager = WorldUtil.Server.EntityManager;
    private NativeParallelHashMap<PrefabGUID, BlueprintData> _blueprintHashLookupMap = WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>().BlueprintHashLookupMap;

    private Dictionary<PrefabGUID, SowSeedPrivs> _seedPrivLookup = [];
    private Dictionary<PrefabGUID, PlantTreePrivs> _saplingPrivLookup = [];

    public GardenService()
    {
        InitSeedPrivLookup();
        InitSaplingPrivLookup();
    }

    public bool IsSeed(PrefabGUID prefabGUID)
    {
        return _seedPrivLookup.ContainsKey(prefabGUID);
    }

    public bool IsSapling(PrefabGUID prefabGUID)
    {
        return _saplingPrivLookup.ContainsKey(prefabGUID);
    }

    public CastlePrivileges AssociatedPrivileges(PrefabGUID prefabGUID)
    {
        var privs = new CastlePrivileges();

        if (_seedPrivLookup.TryGetValue(prefabGUID, out var seedPrivs))
        {
            privs.SowSeed |= seedPrivs;
        }

        if (_saplingPrivLookup.TryGetValue(prefabGUID, out var saplingPrivs))
        {
            privs.PlantTree |= saplingPrivs;
        }

        return privs;
    }

    private void InitSeedPrivLookup()
    {
        _seedPrivLookup = new()
        {
            {
                PrefabGuids.BP_Castle_Chain_Plant_BloodRose,
                SowSeedPrivs.BloodRose
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_FireBlossom,
                SowSeedPrivs.FireBlossom
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_SnowFlower,
                SowSeedPrivs.SnowFlower
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_HellsClarion,
                SowSeedPrivs.HellsClarion
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_MourningLily,
                SowSeedPrivs.MourningLily
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_Sunflower,
                SowSeedPrivs.Sunflower
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_Lotus,
                SowSeedPrivs.PlagueBrier
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_Grapes,
                SowSeedPrivs.Grapes
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_BleedingHeart,
                SowSeedPrivs.BleedingHeart
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_CorruptedFlower,
                SowSeedPrivs.CorruptedFlower
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_GhostShroom,
                SowSeedPrivs.GhostShroom
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_Cotton,
                SowSeedPrivs.Cotton
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_TrippyShroom,
                SowSeedPrivs.TrippyShroom
            },
            {
                PrefabGuids.BP_Castle_Chain_Plant_Thistle,
                SowSeedPrivs.Thistle
            },
        };
    }

    private void InitSaplingPrivLookup()
    {
        _saplingPrivLookup = new()
        {
            {
                PrefabGuids.BP_Castle_Chain_Tree_Spruce_01,
                PlantTreePrivs.Pine
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_Cypress_01,
                PlantTreePrivs.Cypress
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_Aspen_01,
                PlantTreePrivs.Aspen
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_AspenAutum_01,
                PlantTreePrivs.AspenAutumn
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_Birch_01,
                PlantTreePrivs.Birch
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_BirchAutum_01,
                PlantTreePrivs.BirchAutumn
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_AppleTree_01,
                PlantTreePrivs.Apple
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_AppleCursed_01,
                PlantTreePrivs.Cursed
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_GloomTree_01,
                PlantTreePrivs.Gloomy
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_CherryBlossomWhite_01,
                PlantTreePrivs.CherryWhite
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_CherryBlossom_01,
                PlantTreePrivs.Cherry
            },
            {
                PrefabGuids.BP_Castle_Chain_Tree_Oak_01,
                PlantTreePrivs.Oak
            },         
        };
    }



    // It would be better if we could check the item data.
    // There are flags for seeds and saplings...
    // But in practice, saplings are flagged as seeds and not as saplings.
    // So we're stuck manually making lists of things.
    /*
    public unsafe bool IsSeed(PrefabGUID prefabGUID)
    {
        if (!_blueprintHashLookupMap.TryGetValue(prefabGUID, out var blueprintData))
        {
            return false;
        }

        if (!_entityManager.TryGetComponentData<TileData>(blueprintData.Entity, out var tileData))
        {
            return false;
        }

        if (!tileData.Data.IsCreated)
        {
            return false;
        }

        var tileBlob = tileData.Data.Value;
        var placementFlags = tileBlob.AllUsedPlacementFlags;

        var types = placementFlags.Types;
        var requirements = placementFlags.Requirements;
        var restrictions = placementFlags.Restrictions;
        var replaces = placementFlags.Replaces;
        var attachesTo = placementFlags.AttachesTo;

        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"types: {types.ObjectFlags}");
        sb.AppendLine($"requirements: {requirements.ObjectFlags}");
        sb.AppendLine($"restrictions: {restrictions.ObjectFlags}");
        sb.AppendLine($"replaces: {replaces.ObjectFlags}");
        sb.AppendLine($"attaches to: {attachesTo.ObjectFlags}");
        LogUtil.LogDebug(sb.ToString());

        // saplings are flagged as seeds and not as saplings :(
        //var isSeed = 0 != (types.ObjectFlags & PlacementTypeObjectFlags.Seed);
        //var isSapling = 0 != (types.ObjectFlags & PlacementTypeObjectFlags.Sapling);

        // seeds are flagged to go in sapling planters and not seed planters
        // var isSeed = 0 != (attachesTo.ObjectFlags & PlacementTypeObjectFlags.SeedPlanter);
        // var isSapling = 0 != (attachesTo.ObjectFlags & PlacementTypeObjectFlags.SaplingPlanter);

        // seeds are flagged to replace both seeds and saplings. same thing with saplings.
        // but then so are other things like boxes, crafting stations....
        // var isSeed = 0 != (replaces.ObjectFlags & PlacementTypeObjectFlags.SeedPlanter);
        // var isSapling = 0 != (replaces.ObjectFlags & PlacementTypeObjectFlags.SaplingPlanter);

        // I'm beginning to think that "AllUsedPlacementFlags" doesn't refer to "all the categories of flags used by this entity"
        // but maybe it's an aggregate of something?

        LogUtil.LogDebug($"seed: {isSeed} | sapling: {isSapling}");
        return isSeed;
    }
    */

}