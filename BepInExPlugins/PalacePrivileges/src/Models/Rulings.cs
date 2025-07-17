

using Stunlock.Core;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;


public enum RestrictedCastleActions
{
    ExceptionMissingData,
    NotRestricted_SoDoNotCare,
    OpenDoor,
    CloseDoor,
    RenameObject,
    ServantConvert,
    ServantTerminate,
    ServantRename,
    
}


public struct CastleActionRuling
{
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

    public static readonly CastleActionRuling ExceptionMissingData = new()
    {
        IsAllowed = true,
        Action = RestrictedCastleActions.ExceptionMissingData,
    };

    public static readonly CastleActionRuling NotRestricted = new()
    {
        IsAllowed = true,
        Action = RestrictedCastleActions.NotRestricted_SoDoNotCare,
    };
    
}