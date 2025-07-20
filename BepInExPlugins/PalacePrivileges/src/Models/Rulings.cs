

using Stunlock.Core;
using Unity.Collections;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;


public enum RestrictedCastleActions
{
    UnknownAction,
    UnknownDoorAction,
    Irrelevant_SoNotRestricted,
    CastleHeartAbandon,
    CastleHeartExpose,
    CastleHeartRemoveFuel,
    CastleHeartRelocate,
    CastleHeartDisableDefense,
    Build,
    BuildUseTreasury,
    SowSeed,
    PlantTree,
    CraftItem,
    CraftUseTreasury,
    OpenDoor,
    CloseDoor,
    RenameStructure,
    ServantConvert,
    ServantTerminate,
    ServantRename,
    ServantGearChange,
    PrisonerSubdue,
    PrisonerKill,
    PrisonerExtractBlood,
    PrisonerFeedSafeFood,
    PrisonerFeedUnSafeFood,
    PrisonCraftItemUnknown,
    WaygateIn,
    WaygateOut,
    UseTeleporterRed,
    UseTeleporterYellow,
    UseTeleporterPurple,
    UseTeleporterBlue,
    AccessLockbox,
    AccessMusicbox,
    AccessArenaStation,
    AccessThrone,
    AccessRedistributionEngine,
    AccessResearchDeskT1,
    AccessResearchDeskT2,
    AccessResearchDeskT3,
    AccessStygianAltar,
}


public struct CastleActionRuling
{
    public bool NotEnoughDataToDecide;
    public string NotEnoughDataReason;
    public bool IsAllowed;
    public RestrictedCastleActions Action;
    public PrefabGUID TargetPrefabGUID;
    public UserModel ActingUser;
    public UserModel CastleOwner;
    public bool IsOwnerOfCastle;
    public bool IsCastleWithoutOwner;
    public bool IsDefenseDisabled;
    public bool IsSameClan;
    public CastlePrivileges PermissiblePrivs;
    public CastlePrivileges ActingUserPrivs;

    public CastleActionRuling Allowed()
    {
        IsAllowed = true;
        return this;
    }

    public CastleActionRuling Disallowed()
    {
        IsAllowed = false;
        return this;
    }

    public static readonly CastleActionRuling IrrelevantAction = new()
    {
        IsAllowed = true,
        Action = RestrictedCastleActions.Irrelevant_SoNotRestricted,
    };

    public static CastleActionRuling NewRuling_NotEnoughDataToDecide(
        RestrictedCastleActions action = RestrictedCastleActions.UnknownAction,
        string reason = null
    )
    {
        return new()
        {
            NotEnoughDataToDecide = true,
            NotEnoughDataReason = reason,
            IsAllowed = true,
            Action = action,
        };
    }
    
    public static CastleActionRuling NewRuling_NotEnoughDataToDecide(
        RestrictedCastleActions action,
        string reason,
        PrefabGUID targetprefabGUID
    )
    {
        return new()
        {
            NotEnoughDataToDecide = true,
            NotEnoughDataReason = reason,
            IsAllowed = true,
            Action = action,
            TargetPrefabGUID = targetprefabGUID
        };        
    }
    
}