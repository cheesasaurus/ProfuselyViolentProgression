using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;

public struct CastleModel
{
    public bool HasNoOwner;
    public bool IsDefenseDisabled;
    public UserModel Owner;
    public Team Team;
}

public struct CastleDoorModel
{
    public PrefabGUID PrefabGUID;
    public CastleModel Castle;
    public Team Team;
    public CastlePrivileges AcceptablePrivilegesToOpen;
}
