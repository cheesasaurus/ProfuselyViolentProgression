using System.Runtime.InteropServices;
using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Terrain;
using ProjectM.Tiles;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;


/// <summary>
/// Knows general things about castles, but not specifics of things in them.
/// Knowledge of specifics would be services like CastleDoorService, CastleTeleporterService, etc
/// </summary>
public class CastleService
{
    private EntityManager _entityManager = WorldUtil.Server.EntityManager;
    private ManualLogSource _log;
    private UserService _userService;

    public CastleService(ManualLogSource log, UserService userService)
    {
        _log = log;
        _userService = userService;
    }

    public bool TryGetCastleModel_ForConnectedEntity(Entity connectedEntity, out CastleModel castleModel)
    {
        castleModel = default;
        if (!_entityManager.TryGetComponentData<CastleHeartConnection>(connectedEntity, out var castleHeartConnection))
        {
            return false;
        }

        return TryGetCastleModel(castleHeartConnection.CastleHeartEntity._Entity, out castleModel);
    }

    public bool TryGetCastleModel(Entity castleHeartEntity, out CastleModel castleModel)
    {
        castleModel = default;

        if (!_entityManager.TryGetComponentData<CastleHeart>(castleHeartEntity, out var castleHeart))
        {
            return false;
        }

        if (!_entityManager.TryGetComponentData<Team>(castleHeartEntity, out var team))
        {
            return false;
        }

        castleModel.IsDefenseDisabled = castleHeart.IsRaided();
        castleModel.HasNoOwner = !_userService.TryGetUserModel_ForOwnedEntity(castleHeartEntity, out var owner);
        castleModel.Owner = owner;
        castleModel.Team = team;

        return true;
    }

    public bool TryGetCastleHeartOfTerritory_WhereCharacterIs(Entity character, [In] ref MapZoneCollection mapZoneCollection, out Entity castleHeart)
    {
        castleHeart = Entity.Null;

        if (!_entityManager.TryGetComponentData<Translation>(character, out var position))
        {
            return false;
        }

        return TryGetCastleHeartOfTerritory_AtPosition(position, ref mapZoneCollection, out castleHeart);
    }

    public bool TryGetCastleHeartOfTerritory_AtPosition(Translation worldPosition, [In] ref MapZoneCollection mapZoneCollection, out Entity castleHeart)
    {
        castleHeart = Entity.Null;

        var tilePosition = SpaceConversion.WorldToTile(worldPosition.Value);

        if (!mapZoneCollection.TryGetCastleTerritory(ref _entityManager, tilePosition.xz, out CastleTerritory castleTerritory))
        {
            return false;
        }

        castleHeart = castleTerritory.CastleHeart;
        return true;
    }

}