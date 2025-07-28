using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
    GardenService _gardenService;
    PrisonService _prisonService;
    EntityManager _entityManager = WorldUtil.Server.EntityManager;

    public RestrictionService(
        ManualLogSource log,
        GlobalSettingsService globalSettingsService,
        CastlePrivilegesService castlePrivilegesService,
        UserService userService,
        AntiCheatService antiCheatService,
        RulingLoggerService rulingLoggerService,
        CastleService castleService,
        CastleDoorService doorService,
        GardenService gardenService,
        PrisonService prisonService
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
        _gardenService = gardenService;
        _prisonService = prisonService;
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
    #region Castle Heart actions

    private CastlePrivileges PermissiblePrivsTo_CastleHeartAbandon = CastlePrivileges.None; // only owner; no privileges can be granted for this.
    private CastlePrivileges PermissiblePrivsTo_CastleHeartExpose = CastlePrivileges.None; // only owner; no privileges can be granted for this.
    private CastlePrivileges PermissiblePrivsTo_CastleHeartRemoveFuel = CastlePrivileges.None; // only owner; no privileges can be granted for this.
    private CastlePrivileges PermissiblePrivsTo_CastleHeartRelocate = CastlePrivileges.None; // only owner; no privileges can be granted for this.

    public CastleActionRuling ValidateAction_CastleHeartAbandon(Entity actingCharacter, Entity castleHeartEntity)
    {
        var ruling = Internal_ValidateAction_ClanOnlyAtCastleHeart(
            actingCharacter,
            castleHeartEntity,
            RestrictedCastleActions.CastleHeartAbandon,
            PermissiblePrivsTo_CastleHeartAbandon
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_CastleHeartExpose(Entity actingCharacter, Entity castleHeartEntity)
    {
        var ruling = Internal_ValidateAction_ClanOnlyAtCastleHeart(
            actingCharacter,
            castleHeartEntity,
            RestrictedCastleActions.CastleHeartExpose,
            PermissiblePrivsTo_CastleHeartExpose
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_CastleHeartRemoveFuel(Entity actingCharacter, Entity castleHeartEntity)
    {
        var ruling = Internal_ValidateAction_ClanOnlyAtCastleHeart(
            actingCharacter,
            castleHeartEntity,
            RestrictedCastleActions.CastleHeartRemoveFuel,
            PermissiblePrivsTo_CastleHeartRemoveFuel
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_CastleHeartRelocate(Entity actingCharacter, Entity castleHeartEntity)
    {
        var ruling = Internal_ValidateAction_ClanOnlyAtCastleHeart(
            actingCharacter,
            castleHeartEntity,
            RestrictedCastleActions.CastleHeartRelocate,
            PermissiblePrivsTo_CastleHeartRelocate
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastleActionRuling Internal_ValidateAction_ClanOnlyAtCastleHeart(
        Entity actingCharacter,
        Entity castleHeartEntity,
        RestrictedCastleActions action,
        CastlePrivileges permissiblePrivs
    )
    {
        if (!TryGetDataForHeartAction(actingCharacter, castleHeartEntity, out var actingUser, out var castleModel, out var targetPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action);
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = targetPrefabGUID;
        ruling.Action = action;
        ruling.PermissiblePrivs = permissiblePrivs;
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

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    private bool TryGetDataForHeartAction(
        Entity actingCharacter,
        Entity castleHeartEntity,
        out UserModel actingUser,
        out CastleModel castleModel,
        out PrefabGUID targetPrefabGUID
    )
    {
        castleModel = default;
        targetPrefabGUID = default;

        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out actingUser))
        {
            return false;
        }

        if (!_castleService.TryGetCastleModel(castleHeartEntity, out castleModel))
        {
            return false;
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(castleHeartEntity, out targetPrefabGUID))
        {
            return false;
        }

        return true;
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
        if (!TryGetDataForHeartAction(actingCharacter, castleHeartEntity, out var actingUser, out var castleModel, out var targetPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.CastleHeartDisableDefense);
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = targetPrefabGUID;
        ruling.Action = RestrictedCastleActions.CastleHeartDisableDefense;
        ruling.PermissiblePrivs = CastlePrivileges.None;
        HydrateRuling(ref ruling, actingUser, castleModel);
        ruling.IsAllowed = ruling.IsCastleWithoutOwner || !IsFormerClanMemberOnKeyCooldown(ruling);
        return ruling;
    }

    private bool IsFormerClanMemberOnKeyCooldown(CastleActionRuling ruling)
    {
        if (ruling.IsCastleWithoutOwner || ruling.IsSameClan)
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
    #region Build

    private CastlePrivileges PermissiblePrivsTo_Build = new()
    {
        Build = BuildPrivs.UnlistedTBD,
    };

    public CastleActionRuling ValidateAction_BuildPlaceObject(
        Entity actingCharacter,
        PrefabGUID objectToPlacePrefabGUID,
        Entity castleHeart
    )
    {
        var ruling = Internal_ValidateAction_Build(actingCharacter, objectToPlacePrefabGUID, castleHeart);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_BuildStartEdit(
        Entity actingCharacter,
        Entity objectToEdit,
        CastleHeartConnection castleHeartConnection
    )
    {
        var ruling = Internal_ValidateAction_Build(actingCharacter, objectToEdit, castleHeartConnection.CastleHeartEntity._Entity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_BuildDismantle(
        Entity actingCharacter,
        Entity objectToDismantle,
        CastleHeartConnection castleHeartConnection
    )
    {
        var ruling = Internal_ValidateAction_Build(actingCharacter, objectToDismantle, castleHeartConnection.CastleHeartEntity._Entity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_BuildWallpaper(
        Entity actingCharacter,
        Entity objectToEdit,
        CastleHeartConnection castleHeartConnection
    )
    {
        var ruling = Internal_ValidateAction_Build(actingCharacter, objectToEdit, castleHeartConnection.CastleHeartEntity._Entity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_BuildSetVariation(
        Entity actingCharacter,
        Entity objectToEdit,
        CastleHeartConnection castleHeartConnection
    )
    {
        var ruling = Internal_ValidateAction_Build(actingCharacter, objectToEdit, castleHeartConnection.CastleHeartEntity._Entity);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastleActionRuling Internal_ValidateAction_Build(
        Entity actingCharacter,
        Entity objectToBuild,
        Entity castleHeart
    )
    {
        if (!_entityManager.TryGetComponentData<PrefabGUID>(objectToBuild, out var objectPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.Build, "Failed to get PrefabGUID of objectToBuild");
        }
        return Internal_ValidateAction_Build(actingCharacter, objectPrefabGUID, castleHeart);
    }


    private CastleActionRuling Internal_ValidateAction_Build(
        Entity actingCharacter,
        PrefabGUID objectPrefabGUID,
        Entity castleHeart
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.Build, "Failed to get User model for acting character");
        }

        if (!_castleService.TryGetCastleModel(castleHeart, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.Build, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = objectPrefabGUID;
        ruling.Action = RestrictedCastleActions.Build;
        ruling.PermissiblePrivs = PermissiblePrivsTo_Build;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            _antiCheatService.Detected_BadBuilder(actingUser);
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region BuildUseTreasury

    public CastleActionRuling ValidateAction_BuildUseTreasury(Entity actingCharacter, [In] ref MapZoneCollection mapZoneCollection)
    {
        var ruling = Internal_ValidateAction_BuildUseTreasury(actingCharacter, ref mapZoneCollection);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_BuildUseTreasury = new()
    {
        Build = BuildPrivs.UseTreasury,
    };

    private CastleActionRuling Internal_ValidateAction_BuildUseTreasury(Entity actingCharacter, [In] ref MapZoneCollection mapZoneCollection)
    {
        if (!_castleService.TryGetCastleHeartOfTerritory_WhereCharacterIs(actingCharacter, ref mapZoneCollection, out var castleHeartEntity))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.BuildUseTreasury, "Failed to get castle heart of territory where acting character is");
        }

        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.BuildUseTreasury, "Failed to get User model for acting character");
        }

        if (!_castleService.TryGetCastleModel(castleHeartEntity, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.BuildUseTreasury, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.BuildUseTreasury;
        ruling.TargetPrefabGUID = default;
        ruling.PermissiblePrivs = PermissiblePrivsTo_BuildUseTreasury;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region Placing seeds and saplings

    public CastleActionRuling ValidateAction_SowSeed(Entity actingCharacter, PrefabGUID seedPrefabGUID, Entity castleHeart)
    {
        var ruling = Internal_ValidateAction_PlaceSeedOrSapling(
            actingCharacter,
            seedPrefabGUID,
            castleHeart,
            RestrictedCastleActions.SowSeed,
            _gardenService.AssociatedPrivileges(seedPrefabGUID)
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_PlantTree(Entity actingCharacter, PrefabGUID treePrefabGUID, Entity castleHeart)
    {
        var ruling = Internal_ValidateAction_PlaceSeedOrSapling(
            actingCharacter,
            treePrefabGUID,
            castleHeart,
            RestrictedCastleActions.PlantTree,
            _gardenService.AssociatedPrivileges(treePrefabGUID)
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastleActionRuling Internal_ValidateAction_PlaceSeedOrSapling(
        Entity actingCharacter,
        PrefabGUID objectPrefabGUID,
        Entity castleHeart,
        RestrictedCastleActions action,
        CastlePrivileges permissiblePrivs
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_castleService.TryGetCastleModel(castleHeart, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = objectPrefabGUID;
        ruling.PermissiblePrivs = permissiblePrivs;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
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
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.UnknownDoorAction, "Failed to get User model for acting character");
        }

        if (!_doorService.TryGetDoorModel(door, out var doorModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.UnknownDoorAction, "Failed to get Door model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = doorModel.IsOpen ? RestrictedCastleActions.CloseDoor : RestrictedCastleActions.OpenDoor;
        ruling.TargetPrefabGUID = doorModel.PrefabGUID;
        ruling.PermissiblePrivs = doorModel.PermissiblePrivsToOpen;
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

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region RenameCastleStructure

    public CastleActionRuling ValidateAction_RenameCastleStructure(
        Entity actingCharacter,
        Entity structureToRename,
        CastleHeartConnection castleHeartConnection
    )
    {
        var ruling = Internal_ValidateAction_RenameCastleStructure(actingCharacter, structureToRename, castleHeartConnection);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_RenameStructure = new()
    {
        Misc = MiscPrivs.RenameStructures,
    };

    private CastleActionRuling Internal_ValidateAction_RenameCastleStructure(
        Entity actingCharacter,
        Entity structureToRename,
        CastleHeartConnection castleHeartConnection
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.RenameStructure, "Failed to get User model for acting character");
        }

        if (!_castleService.TryGetCastleModel(castleHeartConnection.CastleHeartEntity._Entity, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.RenameStructure, "Failed to get Castle model");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(structureToRename, out var objectPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.RenameStructure, "Failed to get prefabGUID of structureToRename");
        }

        var ruling = new CastleActionRuling();
        ruling.TargetPrefabGUID = objectPrefabGUID;
        ruling.Action = RestrictedCastleActions.RenameStructure;
        ruling.PermissiblePrivs = PermissiblePrivsTo_RenameStructure;
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
        if (action == RestrictedCastleActions.Irrelevant_SoNotRestricted)
        {
            return CastleActionRuling.IrrelevantAction;
        }

        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(coffin, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(coffin, out var coffinPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for coffin");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = coffinPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivs_ForServantAction(action);
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
                return RestrictedCastleActions.Irrelevant_SoNotRestricted;
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
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ServantRename, "Failed to get User model for acting character");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(coffin, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ServantRename, "Failed to get Castle model");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(coffin, out var coffinPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ServantRename, "Failed to get prefabGUID for coffin");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.ServantRename;
        ruling.TargetPrefabGUID = coffinPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_ServantRename;
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
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ServantGearChange, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<ServantConnectedCoffin>(servant, out var connectedCoffin))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ServantGearChange, "Failed to get ServantConnectedCoffin");
        }
        var coffin = connectedCoffin.CoffinEntity._Entity;

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(coffin, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ServantGearChange, "Failed to get Castle model");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(servant, out var servantPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ServantGearChange, "Failed to get prefabGUID for servant");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.ServantGearChange;
        ruling.TargetPrefabGUID = servantPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_ServantGearChange;
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

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region castle waygate usage

    public CastleActionRuling ValidateAction_WaygateIn(Entity actingCharacter, Entity toCastleWaygate)
    {
        var ruling = Internal_ValidateAction_CastleWaygate(actingCharacter, toCastleWaygate, RestrictedCastleActions.WaygateIn, PermissiblePrivsTo_WaygateIn);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_WaygateOut(Entity actingCharacter, Entity toCastleWaygate)
    {
        var ruling = Internal_ValidateAction_CastleWaygate(actingCharacter, toCastleWaygate, RestrictedCastleActions.WaygateOut, PermissiblePrivsTo_WaygateOut);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_WaygateIn = new()
    {
        Teleporter = TeleporterPrivs.WaygateIn,
    };

    private CastlePrivileges PermissiblePrivsTo_WaygateOut = new()
    {
        Teleporter = TeleporterPrivs.WaygateOut,
    };

    private CastleActionRuling Internal_ValidateAction_CastleWaygate(
        Entity actingCharacter,
        Entity castleWaygate,
        RestrictedCastleActions action,
        CastlePrivileges permittedPrivs
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(castleWaygate, out var waygatePrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for waygate");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(castleWaygate, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = waygatePrefabGUID;
        ruling.PermissiblePrivs = permittedPrivs;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region access interactive structures

    public CastleActionRuling ValidateAction_AccessLockbox(Entity actingCharacter, Entity lockbox)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            lockbox,
            RestrictedCastleActions.AccessLockbox,
            PermissiblePrivsTo_AccessLockbox,
            isRaidable: false
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessLockbox = new()
    {
        Misc = MiscPrivs.Lockbox,
    };

    public CastleActionRuling ValidateAction_AccessMusicbox(Entity actingCharacter, Entity musicbox)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            musicbox,
            RestrictedCastleActions.AccessMusicbox,
            PermissiblePrivsTo_AccessMusicbox,
            isRaidable: false
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessMusicbox = new()
    {
        Misc = MiscPrivs.Musicbox,
    };

    public CastleActionRuling ValidateAction_AccessArenaStation(Entity actingCharacter, Entity arenaStation)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            arenaStation,
            RestrictedCastleActions.AccessArenaStation,
            PermissiblePrivsTo_AccessArenaStation,
            isRaidable: false
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessArenaStation = new()
    {
        Arena = ArenaPrivs.UseStation,
    };

    public CastleActionRuling ValidateAction_AccessThrone(Entity actingCharacter, Entity throne)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            throne,
            RestrictedCastleActions.AccessThrone,
            PermissiblePrivsTo_AccessThrone,
            isRaidable: false
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessThrone = new()
    {
        Servant = ServantPrivs.Throne,
    };

    public CastleActionRuling ValidateAction_AccessRedistributionEngine(Entity actingCharacter, Entity engine)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            engine,
            RestrictedCastleActions.AccessRedistributionEngine,
            PermissiblePrivsTo_AccessRedistributionEngine,
            isRaidable: false
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessRedistributionEngine = new()
    {
        Redistribution = RedistributionPrivs.EditRoutes,
    };

    public CastleActionRuling ValidateAction_UseTeleporterRed(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.UseTeleporterRed,
            PermissiblePrivsTo_AccessTeleporterRed,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessTeleporterRed = new()
    {
        Teleporter = TeleporterPrivs.Red,
    };

    public CastleActionRuling ValidateAction_UseTeleporterYellow(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.UseTeleporterYellow,
            PermissiblePrivsTo_AccessTeleporterYellow,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessTeleporterYellow = new()
    {
        Teleporter = TeleporterPrivs.Yellow,
    };

    public CastleActionRuling ValidateAction_UseTeleporterPurple(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.UseTeleporterPurple,
            PermissiblePrivsTo_AccessTeleporterPurple,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessTeleporterPurple = new()
    {
        Teleporter = TeleporterPrivs.Purple,
    };

    public CastleActionRuling ValidateAction_UseTeleporterBlue(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.UseTeleporterBlue,
            PermissiblePrivsTo_AccessTeleporterBlue,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessTeleporterBlue = new()
    {
        Teleporter = TeleporterPrivs.Blue,
    };

    public CastleActionRuling ValidateAction_AccessResearchDeskT1(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.AccessResearchDeskT1,
            PermissiblePrivsTo_AccessResearchDeskT1,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessResearchDeskT1 = new()
    {
        Research = ResearchPrivs.DeskTier1,
    };

    public CastleActionRuling ValidateAction_AccessResearchDeskT2(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.AccessResearchDeskT2,
            PermissiblePrivsTo_AccessResearchDeskT2,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessResearchDeskT2 = new()
    {
        Research = ResearchPrivs.DeskTier2,
    };

    public CastleActionRuling ValidateAction_AccessResearchDeskT3(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.AccessResearchDeskT3,
            PermissiblePrivsTo_AccessResearchDeskT3,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessResearchDeskT3 = new()
    {
        Research = ResearchPrivs.DeskTier3,
    };

    public CastleActionRuling ValidateAction_AccessStygianAltar(Entity actingCharacter, Entity teleporter)
    {
        var ruling = Internal_ValidateAction_AccessInteractiveStructure(
            actingCharacter,
            teleporter,
            RestrictedCastleActions.AccessStygianAltar,
            PermissiblePrivsTo_AccessStygianAltar,
            isRaidable: true
        );
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_AccessStygianAltar = new()
    {
        Research = ResearchPrivs.StygianAltar,
    };

    private CastleActionRuling Internal_ValidateAction_AccessInteractiveStructure(
        Entity actingCharacter,
        Entity structure,
        RestrictedCastleActions action,
        CastlePrivileges permittedPrivs,
        bool isRaidable
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(structure, out var structurePrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for interactive structure");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(structure, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = structurePrefabGUID;
        ruling.PermissiblePrivs = permittedPrivs;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsCastleWithoutOwner)
        {
            return ruling.Allowed();
        }

        if (isRaidable && ruling.IsDefenseDisabled)
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

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region prisoner interaction

    public CastleActionRuling ValidateAction_PrisonerSubdue(Entity actingCharacter, Entity prison)
    {
        var ruling = Internal_ValidateAction_InteractWithPrisoner(actingCharacter, prison, RestrictedCastleActions.PrisonerSubdue, PermissiblePrivsTo_PrisonerSubdue);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_PrisonerSubdue = new()
    {
        Prison = PrisonPrivs.Subdue,
    };

    public CastleActionRuling ValidateAction_PrisonerKill(Entity actingCharacter, Entity prison)
    {
        var ruling = Internal_ValidateAction_InteractWithPrisoner(actingCharacter, prison, RestrictedCastleActions.PrisonerKill, PermissiblePrivsTo_PrisonerKill);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_PrisonerKill = new()
    {
        Prison = PrisonPrivs.Kill,
    };

    private CastleActionRuling Internal_ValidateAction_InteractWithPrisoner(
        Entity actingCharacter,
        Entity prison,
        RestrictedCastleActions action,
        CastlePrivileges permittedPrivs
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(prison, out var prisonPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for prison");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(prison, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = prisonPrefabGUID;
        ruling.PermissiblePrivs = permittedPrivs;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            _antiCheatService.Detected_PrisonBreaker(actingUser);
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region prisoner feeding

    public CastleActionRuling ValidateAction_PrisonerFeeding(Entity actingCharacter, Entity prison, PrefabGUID recipePrefabGUID)
    {
        var ruling = Internal_ValidateAction_PrisonerFeeding_Step1(actingCharacter, prison, recipePrefabGUID);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_PrisonerExtractBlood = new()
    {
        Prison = PrisonPrivs.ExtractBlood,
    };
    private CastlePrivileges PermissiblePrivsTo_PrisonerFeedSafeFood = new()
    {
        Prison = PrisonPrivs.FeedSafeFood,
    };
    private CastlePrivileges PermissiblePrivsTo_PrisonerFeedUnSafeFood = new()
    {
        Prison = PrisonPrivs.FeedUnSafeFood,
    };

    private CastleActionRuling Internal_ValidateAction_PrisonerFeeding_Step1(
        Entity actingCharacter,
        Entity prison,
        PrefabGUID recipePrefabGUID
    )
    {
        var recipeCategory = _prisonService.DetermineRecipeCategory(recipePrefabGUID);
        RestrictedCastleActions action;
        CastlePrivileges permissiblePrivs;

        switch (recipeCategory)
        {
            case PrisonService.PrisonRecipeCategory.ExtractBlood:
                action = RestrictedCastleActions.PrisonerExtractBlood;
                permissiblePrivs = PermissiblePrivsTo_PrisonerExtractBlood;
                break;

            case PrisonService.PrisonRecipeCategory.SafeFood:
                action = RestrictedCastleActions.PrisonerFeedSafeFood;
                permissiblePrivs = PermissiblePrivsTo_PrisonerFeedSafeFood;
                break;

            case PrisonService.PrisonRecipeCategory.UnsafeFood:
                action = RestrictedCastleActions.PrisonerFeedUnSafeFood;
                permissiblePrivs = PermissiblePrivsTo_PrisonerFeedUnSafeFood;
                break;

            default:
            case PrisonService.PrisonRecipeCategory.Unknown:
                return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.PrisonCraftItemUnknown, "Unrecognized prison recipe", recipePrefabGUID);
        }

        return Internal_ValidateAction_PrisonerFeeding_Step2(actingCharacter, prison, recipePrefabGUID, action, ref permissiblePrivs);
    }

    private CastleActionRuling Internal_ValidateAction_PrisonerFeeding_Step2(
        Entity actingCharacter,
        Entity prison,
        PrefabGUID recipePrefabGUID,
        RestrictedCastleActions action,
        [In] ref CastlePrivileges permittedPrivs
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character", recipePrefabGUID);
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(prison, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model", recipePrefabGUID);
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = recipePrefabGUID;
        ruling.PermissiblePrivs = permittedPrivs;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            _antiCheatService.Detected_PrisonBreaker(actingUser);
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region crafting

    public CastleActionRuling ValidateAction_CraftItem(Entity actingCharacter, Entity craftingStation, PrefabGUID recipePrefabGUID)
    {
        var ruling = Internal_ValidateAction_CraftItem(actingCharacter, craftingStation, recipePrefabGUID);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_CraftItem = new()
    {
        Craft = CraftPrivs.CraftItem,
    };

    private CastleActionRuling Internal_ValidateAction_CraftItem(
        Entity actingCharacter,
        Entity craftingStation,
        PrefabGUID recipePrefabGUID
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.CraftItem, "Failed to get User model for acting character", recipePrefabGUID);
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(craftingStation, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.CraftItem, "Failed to get Castle model", recipePrefabGUID);
        }

        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.CraftItem;
        ruling.TargetPrefabGUID = recipePrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_CraftItem;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region arena zone painting

    public CastleActionRuling ValidateAction_ArenaPaintZone(Entity actingCharacter, Entity arenaStation)
    {
        var ruling = Internal_ValidateAction_ArenaPaintZone(actingCharacter, arenaStation);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_ArenaPaintZone = new()
    {
        Arena = ArenaPrivs.ZonePainting,
    };

    private CastleActionRuling Internal_ValidateAction_ArenaPaintZone(
        Entity actingCharacter,
        Entity arenaStation
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ArenaPaintZone, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(arenaStation, out var arenaStationPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ArenaPaintZone, "Failed to get prefabGUID for arenaStation");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(arenaStation, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.ArenaPaintZone, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = RestrictedCastleActions.ArenaPaintZone;
        ruling.TargetPrefabGUID = arenaStationPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_ArenaPaintZone;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region redistribution route editing

    public CastleActionRuling ValidateAction_RedistributionRouteAdd(Entity actingCharacter, Entity fromStation)
    {
        var ruling = Internal_ValidateAction_RedistributionEditRoute(actingCharacter, fromStation, RestrictedCastleActions.RedistributionRouteAdd);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_RedistributionRouteRemove(Entity actingCharacter, Entity fromStation)
    {
        var ruling = Internal_ValidateAction_RedistributionEditRoute(actingCharacter, fromStation, RestrictedCastleActions.RedistributionRouteRemove);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_RedistributionRouteReorder(Entity actingCharacter, Entity fromStation)
    {
        var ruling = Internal_ValidateAction_RedistributionEditRoute(actingCharacter, fromStation, RestrictedCastleActions.RedistributionRouteReorder);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_RedistributionClearRoutes(Entity actingCharacter, Entity engine)
    {
        var ruling = Internal_ValidateAction_RedistributionEditRoute(actingCharacter, engine, RestrictedCastleActions.RedistributionRoutesClear);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_RedistributionEditRoute = new()
    {
        Redistribution = RedistributionPrivs.EditRoutes,
    };

    private CastleActionRuling Internal_ValidateAction_RedistributionEditRoute(
        Entity actingCharacter,
        Entity station,
        RestrictedCastleActions action
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(station, out var stationPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for station");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(station, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = stationPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_RedistributionEditRoute;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region redistribution toggle auto send

    public CastleActionRuling ValidateAction_RedistributionToggleAutoSend(Entity actingCharacter, Entity station)
    {
        var ruling = Internal_ValidateAction_RedistributionToggleAutoSend(actingCharacter, station, RestrictedCastleActions.RedistributionToggleAutoSend);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_RedistributionToggleAutoSend = new()
    {
        Redistribution = RedistributionPrivs.ToggleAutoSend,
    };

    private CastleActionRuling Internal_ValidateAction_RedistributionToggleAutoSend(
        Entity actingCharacter,
        Entity station,
        RestrictedCastleActions action
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(station, out var stationPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for station");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(station, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = stationPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_RedistributionToggleAutoSend;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region redistribution quick send

    public CastleActionRuling ValidateAction_RedistributionQuickSend(Entity actingCharacter, Entity fromStation)
    {
        var ruling = Internal_ValidateAction_RedistributionQuickSend(actingCharacter, fromStation, RestrictedCastleActions.RedistributionQuickSend);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_RedistributionQuickSend = new()
    {
        Redistribution = RedistributionPrivs.QuickSend,
    };

    private CastleActionRuling Internal_ValidateAction_RedistributionQuickSend(
        Entity actingCharacter,
        Entity fromStation,
        RestrictedCastleActions action
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(fromStation, out var stationPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for station");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(fromStation, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = stationPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_RedistributionQuickSend;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #region toggle refining

    public CastleActionRuling ValidateAction_ToggleRefinement(Entity actingCharacter, Entity fromStation)
    {
        var ruling = Internal_ValidateAction_ToggleRefinement(actingCharacter, fromStation, RestrictedCastleActions.ToggleRefinement);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    public CastleActionRuling ValidateAction_ToggleRefinementRecipe(Entity actingCharacter, Entity fromStation, PrefabGUID recipePrefabGUID)
    {
        var ruling = Internal_ValidateAction_ToggleRefinement(actingCharacter, fromStation, RestrictedCastleActions.ToggleRefinement);
        _rulingLoggerService.LogRuling(ruling);
        return ruling;
    }

    private CastlePrivileges PermissiblePrivsTo_ToggleRefinement = new()
    {
        Misc = MiscPrivs.ToggleRefinement,
    };

    private CastleActionRuling Internal_ValidateAction_ToggleRefinement(
        Entity actingCharacter,
        Entity fromStation,
        RestrictedCastleActions action
    )
    {
        if (!_userService.TryGetUserModel_ForCharacter(actingCharacter, out var actingUser))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get User model for acting character");
        }

        if (!_entityManager.TryGetComponentData<PrefabGUID>(fromStation, out var stationPrefabGUID))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get prefabGUID for station");
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(fromStation, out var castleModel))
        {
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(action, "Failed to get Castle model");
        }

        var ruling = new CastleActionRuling();
        ruling.Action = action;
        ruling.TargetPrefabGUID = stationPrefabGUID;
        ruling.PermissiblePrivs = PermissiblePrivsTo_ToggleRefinement;
        HydrateRuling(ref ruling, actingUser, castleModel);

        if (ruling.IsOwnerOfCastle)
        {
            return ruling.Allowed();
        }

        if (!ruling.IsSameClan)
        {
            return ruling.Disallowed();
        }

        ruling.IsAllowed = ruling.ActingUserPrivs.Intersects(ruling.PermissiblePrivs);
        return ruling;
    }

    #endregion
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    

}