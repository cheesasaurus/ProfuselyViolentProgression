using BepInEx.Logging;
using ProfuselyViolentProgression.PalacePrivileges.Commands;
using ProfuselyViolentProgression.PalacePrivileges.Services;

namespace ProfuselyViolentProgression.PalacePrivileges;

public static class Core
{
    public static bool IsInitialized { get; private set; } = false;
    public static SCTService SCTService { get; private set; }
    public static UserService UserService { get; private set; }
    public static AntiCheatService AntiCheatService { get; private set; }
    public static CastleService CastleService { get; private set; }
    public static CastleDoorService CastleDoorService { get; private set; }
    public static PrivilegeParser PrivilegeParser { get; private set; }
    public static CastlePrivilegesService CastlePrivilegesService { get; private set; }
    public static RestrictionService RestrictionService { get; private set; }
    

    public static void Initialize(ManualLogSource log)
    {
        IsInitialized = true;

        SCTService = new();
        UserService = new();
        AntiCheatService = new(log);
        CastleService = new(UserService);
        CastleDoorService = new(CastleService);
        PrivilegeParser = new();

        CastlePrivilegesService = new(
            log: log,
            globalSettingsRepo: new(log, MyPluginInfo.PLUGIN_GUID, "GlobalSettings.json"),
            playerSettingsRepo: new(log, MyPluginInfo.PLUGIN_GUID, "PlayerSettings")
        );
        CastlePrivilegesService.LoadSettings();
        Hooks.BeforeWorldSave += CastlePrivilegesService.SaveSettings;

        RestrictionService = new(
            log: log,
            castlePrivilegesService: CastlePrivilegesService,
            userService: UserService,
            antiCheatService: AntiCheatService,
            doorService: CastleDoorService
        );
    }

    public static void Dispose()
    {
        if (!IsInitialized)
        {
            return;
        }
        IsInitialized = false;
        Hooks.BeforeWorldSave -= CastlePrivilegesService.SaveSettings;
    }

    public static void Save()
    {
        CastlePrivilegesService?.SaveSettings();
    }

}