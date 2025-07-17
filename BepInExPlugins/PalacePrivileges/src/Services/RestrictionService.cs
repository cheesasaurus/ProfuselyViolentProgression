using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class RestrictionService
{
    ManualLogSource _log;
    CastlePrivilegesService _castlePrivilegesService;
    UserService _userService;
    AntiCheatService _antiCheatService;
    RulingLoggerService _rulingLoggerService;
    CastleService _castleService;
    CastleDoorService _doorService;
    EntityManager _entityManager = WorldUtil.Server.EntityManager;

    public RestrictionService(
        ManualLogSource log,
        CastlePrivilegesService castlePrivilegesService,
        UserService userService,
        AntiCheatService antiCheatService,
        RulingLoggerService rulingLoggerService,
        CastleService castleService,
        CastleDoorService doorService
    )
    {
        _log = log;
        _castlePrivilegesService = castlePrivilegesService;
        _userService = userService;
        _antiCheatService = antiCheatService;
        _rulingLoggerService = rulingLoggerService;
        _castleService = castleService;
        _doorService = doorService;
    }

    private void HydrateRuling(ref CastleActionRuling ruling, UserModel actingUser, CastleModel castleModel)
    {
        ruling.ActingUser = actingUser;
        ruling.CastleOwner = castleModel.Owner;
        ruling.IsOwnerOfCastle = castleModel.Owner.Equals(actingUser);
        ruling.IsCastleWithoutOwner = castleModel.Owner.Equals(UserModel.Null);
        ruling.IsDefenseDisabled = castleModel.IsDefenseDisabled;
        ruling.IsSameClan = castleModel.Team.Equals(actingUser.Team);
        ruling.ActingUserPrivs = _castlePrivilegesService.OverallPrivilegesForActingPlayerInClan(castleModel.Owner.PlatformId, actingUser.PlatformId);
    }

    public CastleActionRuling ValidateAction_OpenOrCloseDoor(Entity actingCharacter, Entity door)
    {
        var ruling = Internal_ValidateAction_OpenOrCloseDoor(actingCharacter, door);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastleActionRuling Internal_ValidateAction_OpenOrCloseDoor(Entity actingCharacter, Entity door)
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_doorService.TryGetDoorModel(door, out var doorModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.Action = doorModel.IsOpen ? RestrictedCastleActions.CloseDoor : RestrictedCastleActions.OpenDoor;
        ruling.TargetPrefabGUID = doorModel.PrefabGUID;
        HydrateRuling(ref ruling, actingUser, doorModel.Castle);

        if (ruling.IsDefenseDisabled || ruling.IsCastleWithoutOwner)
        {
            return ruling.Allowed();
        }

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            _antiCheatService.Detected_DoorLockPicker(actingUser);
            return ruling.Disallowed();
        }

        ruling.PermissiblePrivs = doorModel.PermissiblePrivsToOpen;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(doorModel.PermissiblePrivsToOpen);
        return ruling;
    }

    public CastleActionRuling ValidateAction_RenameCastleObject(
        Entity actingCharacter,
        Entity objectToRename,
        CastleHeartConnection castleHeartConnection
    )
    {
        var ruling = Internal_ValidateAction_RenameCastleObject(actingCharacter, objectToRename, castleHeartConnection);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsToRenameObject = new()
    {
        Misc = MiscPrivs.RenameObjects,
    };
    
    private CastleActionRuling Internal_ValidateAction_RenameCastleObject(
        Entity actingCharacter,
        Entity objectToRename,
        CastleHeartConnection castleHeartConnection
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_castleService.TryGetCastleModel(castleHeartConnection.CastleHeartEntity._Entity, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(objectToRename, out var objectPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = objectPrefabGUID;
        ruling.Action = RestrictedCastleActions.RenameObject;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsCastleWithoutOwner)
        {
            return ruling.Allowed();
        }

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            _antiCheatService.Detected_NaughtyNamer(actingUser);
            return ruling.Disallowed();
        }

        ruling.PermissiblePrivs = PermissiblePrivsToRenameObject;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(PermissiblePrivsToRenameObject);
        return ruling;
    }

    


}