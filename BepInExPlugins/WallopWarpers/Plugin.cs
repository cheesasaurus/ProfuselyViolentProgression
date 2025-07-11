using System;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HookDOTS;
using Il2CppInterop.Runtime;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Linq;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.WallopWarpers;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    HookDOTS.API.HookDOTS _hookDOTS;

    public override void Load()
    {
        // Plugin startup logic
        LogUtil.Init(Log);
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        DebugThingsToCharacter("Dingus");
    }

    public override bool Unload()
    {
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

    private void DebugThingsToCharacter(string targetCharacterName)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var query = EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<PlayerCharacter>(),
            },
        });

        var entities = query.ToEntityArray(Allocator.Temp);
        var playerCharacters = query.ToComponentDataArray<PlayerCharacter>(Allocator.Temp);
        //var components = query.ToComponentDataArray<DealDamageEvent>(Allocator.Temp);
        for (var i = 0; i < entities.Length; i++)
        {
            var playerCharacter = playerCharacters[i];
            if (!playerCharacter.Name.Equals(targetCharacterName))
            {
                continue;
            }
            var entity = entities[i];
            LogUtil.LogInfo($"{playerCharacter.Name} ==========================================");
            //DebugUtil.LogComponentTypes(entity);

            // WallopWarpersUtil.ImpairWaypointUse(entity);
            //LetMeBuild(entity);
            //Enable(entity, BuffModificationTypes.AbilityCastImpair);
        }
    }

    public static void On(Entity character, BuffModificationTypes flag)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(character);
        bfs.Value._Value |= (long)flag;
        EntityManager.SetComponentData(character, bfs);
        DebugUtil.LogBuffableFlagState(character);
    }

    public static void Off(Entity character, BuffModificationTypes flag)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(character);
        bfs.Value._Value &= ~(long)flag;
        EntityManager.SetComponentData(character, bfs);
        DebugUtil.LogBuffableFlagState(character);
    }

}
