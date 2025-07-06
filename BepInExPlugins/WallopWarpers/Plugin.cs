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

        //CheckCharacter("Dingus");
        //CheckThings();
        //TrySomething();

        TrySomething3();
    }

    public override bool Unload()
    {
        _hookDOTS.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

    private void CheckThings()
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var query = EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<CastleWaypoint>(),
            },
        });

        var entities = query.ToEntityArray(Allocator.Temp);
        //var components = query.ToComponentDataArray<DealDamageEvent>(Allocator.Temp);
        for (var i = 0; i < entities.Length; i++)
        {
            LogUtil.LogInfo("==========================================");
            var entity = entities[i];
            //DebugUtil.LogComponentTypes(entity);
            //DebugUtil.LogInteractAbilityBuffer(entity);

            //DebugUtil.LogBuffableFlagState(entity);


        }
    }

    private void CheckCharacter(string targetCharacterName)
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


            //ImpairWaypointUse(entity);

            //DebugUtil.LogComponentTypes(entity);
        }
    }

    private void ImpairWaypointUse(Entity entity)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        DebugUtil.LogBuffableFlagState(entity);
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(entity);
        bfs.Value._Value |= (long)BuffModificationTypes.WaypointImpair;
        bfs.Value._Value |= (long)BuffModificationTypes.LocalTeleporterImpaired;
        EntityManager.SetComponentData(entity, bfs);

        DebugUtil.LogBuffableFlagState(entity);
    }

    unsafe private void TrySomething()
    {
        // var sccs = WorldUtil.Server.GetExistingSystemManaged<ServerConsoleCommandSystem>();
        // sccs.ShowSunDamageRays(true);

        var systemTypes = TypeManager.GetSystems();
        var systemTypes2 = systemTypes.Cast<Il2CppSystem.Collections.Generic.IEnumerable<Il2CppSystem.Type>>();
        foreach (var systemType in systemTypes2.ToList())
        {
            var intPtr = (IntPtr)GCHandle.Alloc(systemType);
            var x = IL2CPP.il2cpp_type_get_type(intPtr);
            LogUtil.LogInfo(x);
            LogUtil.LogInfo(Type.GetType(systemType.FullName, true));
            //LogUtil.LogInfo(systemType.FullName);
        }
    }

    unsafe private void TrySomething2()
    {
        var systemTypeIndices = TypeManager.GetSystemTypeIndices();
        foreach (var systemTypeIndex in systemTypeIndices)
        {
            LogUtil.LogInfo(TypeManager.GetSystemName(systemTypeIndex));
        }
    }

    unsafe private void TrySomething3()
    {
        var world = WorldUtil.Server;
        var systemTypeIndices = TypeManager.GetSystemTypeIndices();
        foreach (var systemTypeIndex in systemTypeIndices)
        {
            var systemHandle = world.GetExistingSystem(systemTypeIndex);
            if (!world.Unmanaged.IsSystemValid(systemHandle))
            {
                return;
            }
            var systemType = world.Unmanaged.GetTypeOfSystem(systemHandle);
            LogUtil.LogInfo(systemType.FullName);

            //LogUtil.LogInfo(TypeManager.GetSystemName(systemTypeIndex));
        }
    }

}
