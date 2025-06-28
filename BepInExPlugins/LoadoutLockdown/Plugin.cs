using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Config;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.LoadoutLockdown.Config;

namespace ProfuselyViolentProgression.LoadoutLockdown;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    HookDOTS.API.HookDOTS _hookDOTS;
    ConfigManagerJSON<LoadoutLockdownConfig> ConfigManager;

    // todo: config the config

    private bool _initialized = false;

    public override void Load()
    {
        LogUtil.Init(Log);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        ConfigManager = new ConfigManagerJSON<LoadoutLockdownConfig>(MyPluginInfo.PLUGIN_GUID, "MyConfig.jsonc", Log);
        var presets = "ProfuselyViolentProgression.LoadoutLockdown.resources.presets";
        ConfigManager.CreateMainFile_FromResource($"{presets}.Default.jsonc");
        ConfigManager.CreateExampleFile_FromResource($"{presets}.Default.jsonc", "Example_Default.jsonc", overwrite: true);
        ConfigManager.CreateExampleFile_FromResource($"{presets}.FishersFantasy.jsonc", "Example_FishersFantasy.jsonc", overwrite: true);
        ConfigManager.CreateExampleFile_FromResource($"{presets}.CrutchersCrucible.jsonc", "Example_CrutchersCrucible.jsonc", overwrite: true);
        ConfigManager.CreateExampleFile_FromResource($"{presets}.SweatlordsSwag.jsonc", "Example_SweatlordsSwag.jsonc", overwrite: true);

        Hooks.EarlyUpdateGroup_Updated += OnEarlyUpdate;

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        Hooks.EarlyUpdateGroup_Updated -= OnEarlyUpdate;
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        if (ConfigManager is not null)
        {
            ConfigManager.ConfigUpdated -= HandleConfigChanged;
            ConfigManager.Dispose();
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
        if (ConfigManager.TryLoadConfig(out var config))
        {
            SetUpLoadoutService(config);
        }
        ConfigManager.ConfigUpdated += HandleConfigChanged;
    }

    public void HandleConfigChanged(LoadoutLockdownConfig config)
    {
        SetUpLoadoutService(config);
    }

    public void SetUpLoadoutService(LoadoutLockdownConfig config)
    {
        LoadoutLockdownService.Instance = new LoadoutLockdownService(config);
        LogUtil.LogWarning($"does fishing pole require hotbar slot: {config.RulesByType.FishingPole.RequiresHotbarSlot}");
    }

}
