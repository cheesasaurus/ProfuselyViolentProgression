using BepInEx.Logging;
using ProfuselyViolentProgression.PalacePrivileges.Commands;
using ProfuselyViolentProgression.PalacePrivileges.Services;

namespace ProfuselyViolentProgression.PalacePrivileges;

public static class Core
{
    public static bool IsInitialized { get; private set; } = false;
    public static DoorService DoorService { get; private set; }
    public static PrivilegeParser PrivilegeParser { get; private set; }
    public static CastlePrivilegesService CastlePrivilegesService { get; private set; }

    public static void Initialize(ManualLogSource log)
    {
        IsInitialized = true;

        DoorService = new DoorService();
        PrivilegeParser = new();

        CastlePrivilegesService = new CastlePrivilegesService(
            log: log,
            globalSettingsRepo: new GlobalSettingsRepository(log, MyPluginInfo.PLUGIN_GUID, "GlobalSettings.json"),
            playerSettingsRepo: new PlayerSettingsRepository(log, MyPluginInfo.PLUGIN_GUID, "PlayerSettings.json"),
            doorService: DoorService
        );
        CastlePrivilegesService.LoadSettings();
        // todo: enable saving
        // Hooks.BeforeWorldSave += CastlePrivilegesService.SaveSettings;

        
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

}