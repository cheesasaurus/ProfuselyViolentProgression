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
        _log.LogWarning($"Lockpicker detected: steam:{actingUser.PlatformId} character:{actingUser.CharacterName}");
    }

}
