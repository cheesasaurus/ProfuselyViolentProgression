using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class RestrictionService
{
    ManualLogSource _log;
    CastlePrivilegesService _castlePrivilegesService;
    UserService _userService;
    AntiCheatService _antiCheatService;
    RulingLoggerService _rulingLoggerService;
    CastleDoorService _doorService;
    EntityManager _entityManager = WorldUtil.Server.EntityManager;

    public RestrictionService(
        ManualLogSource log,
        CastlePrivilegesService castlePrivilegesService,
        UserService userService,
        AntiCheatService antiCheatService,
        RulingLoggerService rulingLoggerService,
        CastleDoorService doorService
    )
    {
        _log = log;
        _castlePrivilegesService = castlePrivilegesService;
        _userService = userService;
        _antiCheatService = antiCheatService;
        _rulingLoggerService = rulingLoggerService;
        _doorService = doorService;
    }

    public CastleActionRuling ValidateAction_OpenDoor(Entity actingCharacter, Entity door)
    {
        var ruling = Internal_ValidateAction_OpenDoor(actingCharacter, door);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_CloseDoor(Entity actingCharacter, Entity door)
    {
        var ruling = Internal_ValidateAction_OpenDoor(actingCharacter, door);
        ruling.Action = RestrictedCastleActions.CloseDoor;
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastleActionRuling Internal_ValidateAction_OpenDoor(Entity actingCharacter, Entity door)
    {
        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.OpenDoor;

        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return ruling.Allowed();
        }

        if (!_doorService.TryGetDoorModel(door, out var doorModel))
        {
            return ruling.Allowed();
        }

        HydrateRuling(ref ruling, actingUser, doorModel.Castle);

        if (ruling.IsDefenseDisabled || ruling.IsCastleWithoutOwner)
        {
            return ruling.Allowed();
        }

        if (ruling.IsOwnerOfCastle)
        {
            
            // todo: uncomment after testing clan privs
            // return ruling.Allowed();
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


}