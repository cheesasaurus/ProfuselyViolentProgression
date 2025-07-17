

namespace ProfuselyViolentProgression.PalacePrivileges.Models;


public enum RestrictedCastleActions
{
    ExceptionMissingData,
    OpenDoor,
    CloseDoor,
}


public struct CastleActionRuling
{
    public bool IsAllowed;
    public RestrictedCastleActions Action;
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
    
}