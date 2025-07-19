using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using Unity.Entities;

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
            _log.LogWarning("connectedEntity: no CastleHeartConnection");
            return false;
        }

        return TryGetCastleModel(castleHeartConnection.CastleHeartEntity._Entity, out castleModel);
    }

    public bool TryGetCastleModel(Entity castleHeartEntity, out CastleModel castleModel)
    {
        castleModel = default;

        if (!_entityManager.TryGetComponentData<CastleHeart>(castleHeartEntity, out var castleHeart))
        {
            _log.LogWarning("castle: no CastleHeart");
            return false;
        }

        if (!_entityManager.TryGetComponentData<Team>(castleHeartEntity, out var team))
        {
            _log.LogWarning("castle: no Team");
            return false;
        }

        castleModel.IsDefenseDisabled = castleHeart.IsRaided();
        castleModel.HasNoOwner = !_userService.TryGetUserModel_ForOwnedEntity(castleHeartEntity, out var owner);
        castleModel.Owner = owner;
        castleModel.Team = team;

        return true;
    }
    
    public bool TryGetCastleHeartOfTerritory_WhereCharacterIs(Entity character, out Entity castleHeart)
    {
        castleHeart = Entity.Null;
        _log.LogWarning("Could not find castle of territory where character is");
        // todo: implement
        return false;
    }

}