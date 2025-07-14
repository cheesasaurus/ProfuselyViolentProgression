using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using VampireCommandFramework;

namespace ProfuselyViolentProgression.PalacePrivileges;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
[BepInDependency("gg.deca.VampireCommandFramework")]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    HookDOTS.API.HookDOTS _hookDOTS;

    public override void Load()
    {
        LogUtil.Init(Log);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        CommandRegistry.RegisterAll();

        Hooks.EarlyUpdateGroup_Updated += OnEarlyUpdate;

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        Hooks.EarlyUpdateGroup_Updated -= OnEarlyUpdate;
        CommandRegistry.UnregisterAssembly();
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        Core.Dispose();
        return true;
    }

    public void OnEarlyUpdate()
    {
        if (!Core.IsInitialized && WorldUtil.IsServerInitialized)
        {
            Core.Initialize(Log);
        }
    }

}
