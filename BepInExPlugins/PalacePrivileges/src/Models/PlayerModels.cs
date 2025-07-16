using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;

public struct UserModel
{
    public FixedString64Bytes CharacterName;
    public ulong PlatformId { get; set; }
    public Team Team { get; set; }
}
