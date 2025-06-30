using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Config;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.LoadoutLockdown.Config;
using ProjectM;
using VampireCommandFramework;

namespace ProfuselyViolentProgression.LoadoutLockdown;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
[BepInDependency("gg.deca.VampireCommandFramework")]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    HookDOTS.API.HookDOTS _hookDOTS;

    ConfigEntry<string> RulesetFilename;
    BepInExConfigReloader BepInExConfigReloader;

    ConfigManagerJSON<LoadoutLockdownConfig> RulesetManager;

    private bool _initialized = false;

    ServerGameSettingsSystem _serverGameSettingsSystem => WorldUtil.Server.GetExistingSystemManaged<ServerGameSettingsSystem>();

    public Plugin() : base()
    {
        LogUtil.Init(Log);
        RulesetFilename = Config.Bind("General", "RulesetFilename", "MyRuleset.jsonc", "Filename of the ruleset to apply. Relative to BepInEx/config/LoadoutLockdown/");
    }

    public override void Load()
    {
        BepInExConfigReloader = new BepInExConfigReloader(Config);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        ResetRulesetManager();
        var presets = "ProfuselyViolentProgression.LoadoutLockdown.resources.presets";
        RulesetManager.CreateMainFile_FromResource($"{presets}.Default.jsonc");
        RulesetManager.CreateExampleFile_FromResource($"{presets}.Default.jsonc", "Example_Default.jsonc", overwrite: true);
        RulesetManager.CreateExampleFile_FromResource($"{presets}.FishersFantasy.jsonc", "Example_FishersFantasy.jsonc", overwrite: true);
        RulesetManager.CreateExampleFile_FromResource($"{presets}.CrutchersCrucible.jsonc", "Example_CrutchersCrucible.jsonc", overwrite: true);
        RulesetManager.CreateExampleFile_FromResource($"{presets}.SweatlordsSwag.jsonc", "Example_SweatlordsSwag.jsonc", overwrite: true);

        Hooks.EarlyUpdateGroup_Updated += OnEarlyUpdate;
        RulesetFilename.SettingChanged += HandleRulesetFilenameChanged;

        CommandRegistry.RegisterAll();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        CommandRegistry.UnregisterAssembly();
        BepInExConfigReloader?.Dispose();
        Hooks.EarlyUpdateGroup_Updated -= OnEarlyUpdate;
        RulesetFilename.SettingChanged -= HandleRulesetFilenameChanged;
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        if (RulesetManager is not null)
        {
            RulesetManager.ConfigUpdated -= HandleRulesetChanged;
            RulesetManager.Dispose();
        }
        return true;
    }

    public void OnEarlyUpdate()
    {
        if (_initialized || !WorldUtil.IsServerInitialized)
        {
            return;
        }
        _initialized = true;
        ReloadRuleset();
    }

    public void HandleRulesetChanged(LoadoutLockdownConfig config)
    {
        if (!_initialized)
        {
            return;
        }
        Log.LogMessage($"Reloading ruleset {RulesetFilename.Value}");
        ResetLoadoutService(config);
    }

    private void HandleRulesetFilenameChanged(object sender, EventArgs e)
    {
        Log.LogMessage($"Switching ruleset to {RulesetFilename.Value}");
        ResetRulesetManager();
        if (_initialized)
        {
            ReloadRuleset();
        }
    }

    public void ResetRulesetManager()
    {
        if (RulesetManager is not null)
        {
            RulesetManager.ConfigUpdated -= HandleRulesetChanged;
            RulesetManager.Dispose();
        }
        RulesetManager = new ConfigManagerJSON<LoadoutLockdownConfig>(MyPluginInfo.PLUGIN_GUID, RulesetFilename.Value, Log);
        RulesetManager.ConfigUpdated += HandleRulesetChanged;
    }

    private void ReloadRuleset()
    {
        if (!RulesetManager.TryLoadConfig(out var config))
        {
            Log.LogWarning("Failed to load ruleset. Falling back to vanilla behaviour.");
            LoadoutLockdownService.Instance = null;
            return;
        }
        ResetLoadoutService(config);
    }

    private void ResetLoadoutService(LoadoutLockdownConfig config)
    {
        LoadoutLockdownService.Instance = new LoadoutLockdownService(config);
        _serverGameSettingsSystem._Settings.WeaponSlots = (byte)config.WeaponSlots;
    }

}
