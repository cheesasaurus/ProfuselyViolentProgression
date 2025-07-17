using ProjectM;
using ProjectM.Network;
using Unity.Collections;

namespace ProfuselyViolentProgression.PalacePrivileges.Models;

public struct UserModel
{
    public FixedString64Bytes CharacterName;
    public ulong PlatformId;
    public Team Team;
    public User User;

    public static readonly UserModel Null = default;
}
