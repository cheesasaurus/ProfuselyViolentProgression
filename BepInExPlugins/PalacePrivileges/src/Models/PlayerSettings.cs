using System.Collections.Generic;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class PlayerSettingsStore
{
    public Dictionary<PlayerIndex, PlayerSettings> SettingsLookup { get; set; }
}

public struct PlayerIndex
{
    public ulong PlatformId { get; set; }
}

public struct PlayerSettings
{
    public CastlePrivileges ClanPrivs { get; set; }
    public Dictionary<PlayerIndex, ActingPlayerPrivileges> PlayerPrivsLookup { get; set; }
}

public struct ActingPlayerPrivileges
{
    public CastlePrivileges Granted { get; set; }
    public CastlePrivileges Forbidden { get; set; }

    /// <summary>
    /// expected to be set by whatever is managing Granted and Forbidden
    /// </summary>
    public CastlePrivileges PrecomputedEffective { get; set; }
}

