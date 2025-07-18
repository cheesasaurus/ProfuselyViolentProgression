using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class RestrictionService
{
    ManualLogSource _log;
    GlobalSettingsService _globalSettingsService;
    CastlePrivilegesService _castlePrivilegesService;
    UserService _userService;
    AntiCheatService _antiCheatService;
    RulingLoggerService _rulingLoggerService;
    CastleService _castleService;
    CastleDoorService _doorService;
    EntityManager _entityManager = WorldUtil.Server.EntityManager;

    public RestrictionService(
        ManualLogSource log,
        GlobalSettingsService globalSettingsService,
        CastlePrivilegesService castlePrivilegesService,
        UserService userService,
        AntiCheatService antiCheatService,
        RulingLoggerService rulingLoggerService,
        CastleService castleService,
        CastleDoorService doorService
    )
    {
        _log = log;
        _globalSettingsService = globalSettingsService;
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

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region CastleHeartAbandon

    public CastleActionRuling ValidateAction_CastleHeartAbandon(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        var ruling = Internal_ValidateAction_CastleHeartAbandon(actingCharacter, castleHeartEntity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_CastleHeartAbandon = CastlePrivileges.None; // only owner; no privileges can be granted for this.

    private CastleActionRuling Internal_ValidateAction_CastleHeartAbandon(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_castleService.TryGetCastleModel(castleHeartEntity, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(castleHeartEntity, out var castleHeartPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = castleHeartPrefabGUID;
        ruling.Action = RestrictedCastleActions.CastleHeartAbandon;
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
            return ruling.Disallowed();
        }

        ruling.PermissiblePrivs = PermissiblePrivsTo_CastleHeartAbandon;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region CastleHeartExpose

    public CastleActionRuling ValidateAction_CastleHeartExpose(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        var ruling = Internal_ValidateAction_CastleHeartExpose(actingCharacter, castleHeartEntity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_CastleHeartExpose = CastlePrivileges.None; // only owner; no privileges can be granted for this.

    private CastleActionRuling Internal_ValidateAction_CastleHeartExpose(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_castleService.TryGetCastleModel(castleHeartEntity, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(castleHeartEntity, out var castleHeartPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = castleHeartPrefabGUID;
        ruling.Action = RestrictedCastleActions.CastleHeartExpose;
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
            return ruling.Disallowed();
        }

        ruling.PermissiblePrivs = PermissiblePrivsTo_CastleHeartExpose;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region CastleHeartRemoveFuel

    public CastleActionRuling ValidateAction_CastleHeartRemoveFuel(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        var ruling = Internal_ValidateAction_CastleHeartRemoveFuel(actingCharacter, castleHeartEntity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_CastleHeartRemoveFuel = CastlePrivileges.None; // only owner; no privileges can be granted for this.

    private CastleActionRuling Internal_ValidateAction_CastleHeartRemoveFuel(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_castleService.TryGetCastleModel(castleHeartEntity, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(castleHeartEntity, out var castleHeartPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = castleHeartPrefabGUID;
        ruling.Action = RestrictedCastleActions.CastleHeartRemoveFuel;
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
            _antiCheatService.Detected_HeartFuelSiphoner(actingUser);
            return ruling.Disallowed();
        }

        ruling.PermissiblePrivs = PermissiblePrivsTo_CastleHeartRemoveFuel;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region CastleHeartDisableDefense

    public CastleActionRuling ValidateAction_CastleHeartDisableDefense(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        var ruling = Internal_ValidateAction_CastleHeartDisableDefense(actingCharacter, castleHeartEntity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastleActionRuling Internal_ValidateAction_CastleHeartDisableDefense(
        Entity actingCharacter,
        Entity castleHeartEntity
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_castleService.TryGetCastleModel(castleHeartEntity, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(castleHeartEntity, out var castleHeartPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = castleHeartPrefabGUID;
        ruling.Action = RestrictedCastleActions.CastleHeartDisableDefense;
        HydrateRuling(ref ruling, actingUser, castleModel);
        ruling.PermissiblePrivs = CastlePrivileges.None;
        ruling.IsAllowed = ruling.IsCastleWithoutOwner || !IsFormerClanMemberOnKeyCooldown(ruling);
        return ruling;
    }

    private bool IsFormerClanMemberOnKeyCooldown(CastleActionRuling ruling)
    {
        if (ruling.IsCastleWithoutOwner)
        {
            return false;
        }

        // todo: actual implementation. will make player history library first.

        bool wereClanmates = false; // todo: actual
        if (!wereClanmates)
        {
            return false;
        }

        var cooldownHours = _globalSettingsService.GetGlobalSettings().KeyClanCooldownHours;
        var cooldown = TimeSpan.FromHours(cooldownHours);

        var timeSeparated = new TimeSpan(hours: 0, minutes: 0, seconds: 0); // todo: actual

        var timeRemaining = cooldown - timeSeparated;
        return timeRemaining > TimeSpan.Zero;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region OpenOrCloseDoor

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
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region RenameCastleObject

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

    private CastlePrivileges PermissiblePrivsTo_RenameObject = new()
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

        ruling.PermissiblePrivs = PermissiblePrivsTo_RenameObject;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region MiscActionsAtCoffin

    public CastleActionRuling ValidateAction_AtCoffin(Entity actingCharacter, Entity coffin, ServantCoffinActionEvent ev)
    {
        var ruling = Internal_ValidateAction_AtCoffin(actingCharacter, coffin, ev);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastleActionRuling Internal_ValidateAction_AtCoffin(Entity actingCharacter, Entity coffin, ServantCoffinActionEvent ev)
    {
        var action = RestrictedCastleAction_FromCoffinAction(ev.Action);
        if (action == RestrictedCastleActions.NotRestricted_SoDoNotCare)
        {
            return CastleActionRuling.NotRestricted;
        }

        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(coffin, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(coffin, out var coffinPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = coffinPrefabGUID;
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
            return ruling.Disallowed();
        }

        ruling.PermissiblePrivs = PermissiblePrivs_ForServantAction(action);
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    private RestrictedCastleActions RestrictedCastleAction_FromCoffinAction(ServantCoffinAction coffinAction)
    {
        switch (coffinAction)
        {
            case ServantCoffinAction.Insert:
                return RestrictedCastleActions.ServantConvert;

            case ServantCoffinAction.Terminate:
                return RestrictedCastleActions.ServantTerminate;

            default:
                return RestrictedCastleActions.NotRestricted_SoDoNotCare;
        }
    }

    private CastlePrivileges PermissiblePrivsTo_ServantConvert = new()
    {
        Servant = ServantPrivs.Convert,
    };

    private CastlePrivileges PermissiblePrivsTo_ServantTerminate = new()
    {
        Servant = ServantPrivs.Terminate,
    };

    private CastlePrivileges PermissiblePrivs_ForServantAction(RestrictedCastleActions action)
    {
        switch (action)
        {
            case RestrictedCastleActions.ServantConvert:
                return PermissiblePrivsTo_ServantConvert;

            case RestrictedCastleActions.ServantTerminate:
                return PermissiblePrivsTo_ServantTerminate;

            default:
                return CastlePrivileges.None;
        }
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region ServantRename

    public CastleActionRuling ValidateAction_ServantRename(Entity actingCharacter, Entity coffin)
    {
        var ruling = Internal_ValidateAction_ServantRename(actingCharacter, coffin);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_ServantRename = new()
    {
        Servant = ServantPrivs.Rename,
    };

    private CastleActionRuling Internal_ValidateAction_ServantRename(Entity actingCharacter, Entity coffin)
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(coffin, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(coffin, out var coffinPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.ServantRename;
        ruling.TargetPrefabGUID = coffinPrefabGUID;
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

        ruling.PermissiblePrivs = PermissiblePrivsTo_ServantRename;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region servant gear changes

    public CastleActionRuling ValidateAction_ServantGearChange(Entity actingCharacter, Entity servant)
    {
        var ruling = Internal_ValidateAction_ServantGearChange(actingCharacter, servant);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_ServantGearChange = new()
    {
        Servant = ServantPrivs.Gear,
    };

    private CastleActionRuling Internal_ValidateAction_ServantGearChange(Entity actingCharacter, Entity servant)
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<ServantConnectedCoffin>(servant, out var connectedCoffin))
        {
            return CastleActionRuling.ExceptionMissingData;
        }
        var coffin = connectedCoffin.CoffinEntity._Entity;

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(coffin, out var castleModel))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(servant, out var servantPrefabGUID))
        {
            return CastleActionRuling.ExceptionMissingData;
        }

        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.ServantGearChange;
        ruling.TargetPrefabGUID = servantPrefabGUID;
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
            return ruling.Disallowed();
        }

        ruling.PermissiblePrivs = PermissiblePrivsTo_ServantGearChange;
        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    


}