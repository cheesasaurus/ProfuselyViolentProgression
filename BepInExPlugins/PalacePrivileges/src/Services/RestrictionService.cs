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
        GardenService gardenService
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
            return CastleActionRuling.NewRuling_NotEnoughDataToDecide(RestrictedCastleActions.RenameStructure, "Failed to prefabGUID of structureToRename");
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

}