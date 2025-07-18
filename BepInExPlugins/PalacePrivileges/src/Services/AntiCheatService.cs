using BepInEx.Logging;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class AntiCheatService
{
    ManualLogSource _log;

    public AntiCheatService(ManualLogSource log)
    {
        _log = log;
    }

    public void Detected_DoorLockPicker(UserModel actingUser)
    {
        _log.LogWarning($"DoorLockPicker detected: steam:{actingUser.PlatformId} character:{actingUser.CharacterName}");
    }

    public void Detected_NaughtyNamer(UserModel actingUser)
    {
        _log.LogWarning($"NaughtyNamer detected: steam:{actingUser.PlatformId} character:{actingUser.CharacterName}");
    }

    public void Detected_HeartFuelSiphoner(UserModel actingUser)
    {
        _log.LogWarning($"HeartFuelSiphoner detected: steam:{actingUser.PlatformId} character:{actingUser.CharacterName}");
    }

}
