using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProfuselyViolentProgression.Core.Config;
using ProfuselyViolentProgression.Core.Utilities;

namespace ProfuselyViolentProgression.WallopWarpers;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
public class Plugin : BasePlugin
{
    public static ConfigEntry<bool> PvPCombat_AllowWaygateUse;
    public static ConfigEntry<int> SpawnProtectionSeconds;
    public static ConfigEntry<bool> SpawnProtection_AllowWaygateUse;

    Harmony _harmony;
    HookDOTS.API.HookDOTS _hookDOTS;
    BepInExConfigReloader BepInExConfigReloader;

    public Plugin() : base()
    {
        LogUtil.Init(Log);
        PvPCombat_AllowWaygateUse = Config.Bind("General", "PvPCombat_AllowWaygateUse", false, "Whether or not to allow waygate use during PvP combat. Allowed in vanilla.");
        SpawnProtectionSeconds = Config.Bind("General", "SpawnProtectionSeconds", 12, "How long the Phasing protection buff lasts, after teleporting in. Only 5 seconds in vanilla.");
        SpawnProtection_AllowWaygateUse = Config.Bind("General", "SpawnProtection_AllowWaygateUse", true, "Whether or not to allow waygate use during Phasing. Not allowed in vanilla.");
    }

    public override void Load()
    {
        BepInExConfigReloader = new BepInExConfigReloader(Config);
        Config.ConfigReloaded += HandleConfigReloaded;

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();
        
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        Config.ConfigReloaded -= HandleConfigReloaded;
        BepInExConfigReloader?.Dispose();
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }    

    private void HandleConfigReloaded(object sender, EventArgs e)
    {
        LogUtil.LogInfo("Reloaded config.");
    }

}
