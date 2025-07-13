using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.LoadoutLockdown.Commands;
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

        PrivilegeParser.Instance = new();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        CommandRegistry.UnregisterAssembly();
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

}
