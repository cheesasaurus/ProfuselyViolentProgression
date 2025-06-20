using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;

namespace ProfuselyViolentProgression.SurgeSuppressor;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
public class Plugin : BasePlugin
{
    private Harmony _harmony;
    private HookDOTS.API.HookDOTS _hookDOTS;
    private readonly SurgeSuppressorConfig cfg;

    public Plugin() : base()
    {
        LogUtil.Init(Log);
        cfg = new SurgeSuppressorConfig(Config);
    }

    public override void Load()
    {
        SurgeSuppressorUtil.SetSettings(cfg.OnlyProtectPlayers.Value, cfg.ThrottleIntervalMilliseconds.Value);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

}
