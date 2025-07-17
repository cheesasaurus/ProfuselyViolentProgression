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
    CastleDoorService _doorService;
    EntityManager _entityManager = WorldUtil.Server.EntityManager;

    public RestrictionService(
        ManualLogSource log,
        CastlePrivilegesService castlePrivilegesService,
        UserService userService,
        AntiCheatService antiCheatService,
        CastleDoorService doorService
    )
    {
        _log = log;
        _castlePrivilegesService = castlePrivilegesService;
        _userService = userService;
        _antiCheatService = antiCheatService;
        _doorService = doorService;
    }

    public bool ShouldDisallowAction_OpenDoor(Entity actingCharacter, Entity door)
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return false;
        }

        if (!_doorService.TryGetDoorModel(door, out var doorModel))
        {
            return false;
        }

        // todo: toggle for permission debugging
        LogUtil.LogDebug($"defense disabled? {doorModel.Castle.IsDefenseDisabled}");
        LogUtil.LogDebug($"has no owner? {doorModel.Castle.HasNoOwner}");

        if (doorModel.Castle.IsDefenseDisabled || doorModel.Castle.IsAbandoned || doorModel.Castle.HasNoOwner)
        {
            return false;
        }

        var isOwner = doorModel.Castle.Owner.Equals(actingUser);
        LogUtil.LogDebug($"is owner? {isOwner}");
        if (isOwner)
        {
            // todo: uncomment after testing clan privs
            //return false;
        }

        bool isClanMember = doorModel.Team.Equals(actingUser.Team);
        LogUtil.LogDebug($"is clan member? {isClanMember}");
        if (!isClanMember)
        {
            _antiCheatService.Detected_DoorLockPicker(actingUser);
            return true;
        }

        var actingPlayerPrivileges = _castlePrivilegesService.OverallPrivilegesForActingPlayerInClan(doorModel.Castle.Owner.PlatformId, actingUser.PlatformId);        
        return !actingPlayerPrivileges.Intersects(doorModel.AcceptablePrivilegesToOpen);
    }

    public bool ShouldDisallowAction_CloseDoor(Entity actingCharacter, Entity door)
    {
        return ShouldDisallowAction_OpenDoor(actingCharacter, door);
    }


}