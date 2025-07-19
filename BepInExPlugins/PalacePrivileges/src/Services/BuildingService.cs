using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding.Placement;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

/// <summary>
/// Knows things about building castles
/// </summary>
public class BuildingService
{
    private EntityManager _entityManager = WorldUtil.Server.EntityManager;
    // private NativeParallelHashMap<PrefabGUID, ItemData> _itemHashLookupMap = WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>().ItemHashLookupMap;
    private NativeParallelHashMap<PrefabGUID, BlueprintData> _blueprintHashLookupMap = WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>().BlueprintHashLookupMap;

    /// <summary>
    /// Checks if a structure could be placed in an enemy castle. For example: bombs, golem stones...
    /// </summary>
    public bool CouldStructureBePlacedInEnemyCastle(PrefabGUID prefabGUID)
    {
        if (!_blueprintHashLookupMap.TryGetValue(prefabGUID, out var blueprintData))
        {
            return false;
        }

        if (!_entityManager.TryGetComponentData<CastleAreaRequirement>(blueprintData.Entity, out var castleAreaRequirement))
        {
            return false;
        }

        return castleAreaRequirement.RequirementType is CastleAreaRequirementType.IgnoreCastleAreas;
    }

    /// <summary>
    /// Checks if a structure can only be placed in an area owned by your team. (e.g. a castle)
    /// </summary>
    public bool IsStructureOnlyAttachableToOwnedArea(PrefabGUID prefabGUID)
    {
        if (!_blueprintHashLookupMap.TryGetValue(prefabGUID, out var blueprintData))
        {
            return false;
        }

        if (!_entityManager.TryGetComponentData<CastleAreaRequirement>(blueprintData.Entity, out var castleAreaRequirement))
        {
            return false;
        }

        return castleAreaRequirement.RequirementType is CastleAreaRequirementType.AttachToOwnedArea;
    }

    public bool IsSeed(PrefabGUID prefabGUID)
    {
        if (!_blueprintHashLookupMap.TryGetValue(prefabGUID, out var blueprintData))
        {
            return false;
        }

        // todo: check TileData component and find our way down to the field TileBlob.AllUsedPlacementFlags
        // something in there should be the PlacementTypeObjectFlags we're looking for.
        // need to find PlacementTypeObjectFlags.Seed

        // todo: implement
        return false;
    }

    public bool IsSapling(PrefabGUID prefabGUID)
    {
        if (!_blueprintHashLookupMap.TryGetValue(prefabGUID, out var blueprintData))
        {
            return false;
        }


        // todo: implement
        return false;
    }

}