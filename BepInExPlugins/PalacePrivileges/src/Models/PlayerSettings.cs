using System.Collections.Generic;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public struct PlayerIndex(ulong platformId)
{
    public ulong PlatformId { get; set; } = platformId;
}

public struct PlayerSettings
{
    public CastlePrivileges ClanPrivs { get; set; }
    public Dictionary<PlayerIndex, ActingPlayerPrivileges> PlayerPrivsLookup { get; set; }

    public ulong Revision { get; set; }
    public ulong RevisionSaved; // not a property, because we don't need to serialize it.
}

public struct ActingPlayerPrivileges
{
    public CastlePrivileges Granted { get; set; }
    public CastlePrivileges Forbidden { get; set; }
}

