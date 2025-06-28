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

    public override void Load()
    {
        LogUtil.Init(Log);
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        ConfigManager = new ConfigManagerJSON<LoadoutLockdownConfig>(MyPluginInfo.PLUGIN_GUID, "MyConfig.jsonc", Log);
        var presets = "ProfuselyViolentProgression.LoadoutLockdown.resources.presets";
        ConfigManager.CreateMainFile_FromResource($"{presets}.Default.jsonc");
        ConfigManager.CreateExampleFile_FromResource($"{presets}.Default.jsonc", "Example_Default.jsonc");
        ConfigManager.CreateExampleFile_FromResource($"{presets}.FishersFantasy.jsonc", "Example_FishersFantasy.jsonc");
        ConfigManager.CreateExampleFile_FromResource($"{presets}.CrutchersCrucible.jsonc", "Example_CrutchersCrucible.jsonc");
        ConfigManager.CreateExampleFile_FromResource($"{presets}.SweatlordsSwag.jsonc", "Example_SweatlordsSwag.jsonc");
        ConfigManager.ConfigUpdated += HandleConfigChanged;

        if (ConfigManager.TryLoadConfig(out var config))
        {
            LoadoutLockdownService.Instance = new LoadoutLockdownService(config);
            LogUtil.LogWarning($"does fishing pole require hotbar slot: {config.RulesByType.FishingPole.RequiresHotbarSlot}");
        }
    }

    public override bool Unload()
    {
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        if (ConfigManager is not null)
        {
            ConfigManager.ConfigUpdated -= HandleConfigChanged;
            ConfigManager.Dispose();
        }
        return true;
    }

    public void HandleConfigChanged(LoadoutLockdownConfig config)
    {
        LoadoutLockdownService.Instance = new LoadoutLockdownService(config);
    }

}
