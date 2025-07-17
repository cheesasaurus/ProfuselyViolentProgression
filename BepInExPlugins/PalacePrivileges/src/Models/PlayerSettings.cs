using System.Collections.Generic;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;


public struct PlayerSettings()
{
    public CastlePrivileges ClanPrivs { get; set; }
    public Dictionary<ulong, ActingPlayerPrivileges> PlayerPrivsLookup { get; set; } = [];

    public ulong Revision { get; set; }
    public ulong RevisionSaved; // not a property, because we don't need to serialize it.
}

public struct ActingPlayerPrivileges
{
    public CastlePrivileges Granted { get; set; }
    public CastlePrivileges Forbidden { get; set; }
}

