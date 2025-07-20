using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Commands;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProfuselyViolentProgression.PalacePrivileges.Services;
using ProjectM.Scripting;

namespace ProfuselyViolentProgression.PalacePrivileges;

public static class Core
{
    public static bool IsInitialized { get; private set; } = false;
    
    public static ServerScriptMapper ServerScriptMapper { get; private set; }
    public static ServerGameManager ServerGameManager => ServerScriptMapper.GetServerGameManager(); // it's a struct, so don't cache it
    public static SingletonService SingletonService { get; private set; }
    public static SCTService SCTService { get; private set; }
    public static UserService UserService { get; private set; }
    public static AntiCheatService AntiCheatService { get; private set; }
    public static CastleTerritoryService CastleTerritoryService { get; private set; }
    public static CastleService CastleService { get; private set; }
    public static CastleDoorService CastleDoorService { get; private set; }
    public static BuildingService BuildingService { get; private set; }
    public static GardenService GardenService { get; private set; }
    public static PrisonService PrisonService { get; private set; }
    public static PrivilegeParser PrivilegeParser { get; private set; }
    public static GlobalSettingsService GlobalSettingsService { get; private set; }
    public static CastlePrivilegesService CastlePrivilegesService { get; private set; }
    public static RestrictionService RestrictionService { get; private set; }
    public static RulingLoggerService RulingLoggerService { get; private set; }
    public static NotificationService NotificationService { get; private set; }

    public static void Initialize(ManualLogSource log)
    {
        IsInitialized = true;

        ServerScriptMapper = WorldUtil.Server.GetExistingSystemManaged<ServerScriptMapper>();
        SingletonService = new();
        SCTService = new();
        UserService = new();
        AntiCheatService = new(log);
        CastleTerritoryService = new();
        CastleService = new(log, UserService);
        CastleDoorService = new(CastleService);
        BuildingService = new();
        GardenService = new();
        PrisonService = new();
        PrivilegeParser = new();

        GlobalSettingsService = new(
            log: log,
            globalSettingsRepo: new(log, MyPluginInfo.PLUGIN_GUID, "GlobalSettings.json")
        );
        GlobalSettingsService.LoadSettings();
        GlobalSettingsService.GlobalSettingsChanged += HandleGlobalSettingsChanged;

        CastlePrivilegesService = new(
            log: log,
            playerSettingsRepo: new(log, MyPluginInfo.PLUGIN_GUID, "PlayerSettings")
        );
        CastlePrivilegesService.LoadSettings();
        Hooks.BeforeWorldSave += CastlePrivilegesService.SaveSettings;

        RulingLoggerService = new(log);
        RulingLoggerService.Enabled = GlobalSettingsService.GetGlobalSettings().DebugLogRulings;

        RestrictionService = new(
            log: log,
            globalSettingsService: GlobalSettingsService,
            castlePrivilegesService: CastlePrivilegesService,
            userService: UserService,
            antiCheatService: AntiCheatService,
            rulingLoggerService: RulingLoggerService,
            castleService: CastleService,
            doorService: CastleDoorService,
            gardenService: GardenService,
            prisonService: PrisonService
        );

        NotificationService = new(SCTService);
    }

    public static void Dispose()
    {
        if (!IsInitialized)
        {
            return;
        }
        IsInitialized = false;
        GlobalSettingsService.GlobalSettingsChanged -= HandleGlobalSettingsChanged;
        Hooks.BeforeWorldSave -= CastlePrivilegesService.SaveSettings;
    }

    public static void Save()
    {
        GlobalSettingsService?.SaveSettings();
        CastlePrivilegesService?.SaveSettings();
    }

    public static void HandleGlobalSettingsChanged(GlobalSettings newSettings)
    {
        RulingLoggerService.Enabled = newSettings.DebugLogRulings;
    }

}