using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Scripting;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.WallopWarpers;

[HarmonyPatch]
public static class Patches
{
    public static EntityManager EntityManager => WorldUtil.Server.EntityManager;

    [HarmonyPatch(typeof(TeleportationRequestSystem), nameof(TeleportationRequestSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void TeleportationRequestSystem_OnUpdate_Prefix(TeleportationRequestSystem __instance)
    {
        var query = __instance._TeleportRequestQuery;

        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<TeleportationRequest>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            var entity = entities[i];
            var ev = events[i];
            if (WallopWarpersUtil.IsInPvpCombat(ev.PlayerEntity))
            {
                EntityManager.DestroyEntity(entity);
                WallopWarpersUtil.SendMessageTeleportDisallowed(ev.PlayerEntity);
            }
        }
    }

    [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void AbilityRunScriptsSystem_Prefix(AbilityRunScriptsSystem __instance)
    {
        ProcessQuery_OnCastStarted(__instance);
    }

    private static void ProcessQuery_OnCastStarted(AbilityRunScriptsSystem __instance)
    {
        var query = __instance._OnCastStartedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            OnCastStarted(entities[i], events[i]);
        }
    }

    private static void OnCastStarted(Entity entity, AbilityCastStartedEvent ev)
    {
        if (!EntityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        if (!EntityManager.TryGetComponentData<PrefabGUID>(ev.Ability, out var abilityPrefabGUID))
        {
            return;
        }

        if (WallopWarpersUtil.IsUseWaypointCast(abilityPrefabGUID) && WallopWarpersUtil.IsInPvpCombat(ev.Character))
        {
            try
            {
                WallopWarpersUtil.SendMessageTeleportDisallowed(ev.Character);
                //WallopWarpersUtil.ImpairWaypointUse(ev.Character);
                //WallopWarpersUtil.InterruptCast1(entity, ev); // this doesn't work
                //WallopWarpersUtil.InterruptCast2(entity, ev); // this doesn't work
                //EntityManager.DestroyEntity(entity); // this doesn't work, and causes jank
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }
    }

    //[EcsSystemUpdatePostfix(typeof(AbilityStartCastingSystem_Server))]
    //unsafe public static void AbilityStartCastingSystem_Server_OnUpdate_Postfix(SystemState* systemState)
    //{
    //    var query = EntityManager.CreateEntityQuery(new EntityQueryDesc()
    //    {
    //        All = new ComponentType[] {
    //            ComponentType.ReadOnly<AbilityCastStartedEvent>(),
    //        },
    //    });
    //
    //    var entities = query.ToEntityArray(Allocator.Temp);
    //    var events = query.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
    //    for (var i = 0; i < events.Length; i++)
    //    {
    //        var ev = events[i];
    //        OnCastStarted(entities[i], ev);
    //    }
    //}

    // todo: fiddle with these
    // TeleportBuffSystem_Server
    // TeleportBuffSpawnSystem


    [HarmonyPatch(typeof(TeleportBuffSpawnSystem), nameof(TeleportBuffSpawnSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void TeleportBuffSpawnSystem_OnUpdate_Prefix(TeleportBuffSpawnSystem __instance)
    {
        var query = __instance.__query_2122398975_0;
        var entities = query.ToEntityArray(Allocator.Temp);
        var buffs = query.ToComponentDataArray<Buff>(Allocator.Temp);
        var teleportBuffs = query.ToComponentDataArray<TeleportBuff>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[0];
            //DebugUtil.LogComponentTypes(entity);
            DebugUtil.LogPrefabGuid(entity);
        }
    }

    [HarmonyPatch(typeof(TeleportBuffSystem_Server), nameof(TeleportBuffSystem_Server.OnUpdate))]
    [HarmonyPrefix]
    public static void TeleportBuffSpawnSystem_OnUpdate_Prefix(TeleportBuffSystem_Server __instance)
    {
        var query = __instance.__query_2122398833_1;
        var entities = query.ToEntityArray(Allocator.Temp);
        var buffs = query.ToComponentDataArray<Buff>(Allocator.Temp);
        var teleportBuffs = query.ToComponentDataArray<TeleportBuff>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[0];
            //DebugUtil.LogComponentTypes(entity);
            //LogUtil.LogDebug("------------------------");
            //DebugUtil.LogPrefabGuid(entity);
            //DebugUtil.LogSpawnPrefabOnDestroy(entity);
        }
    }

    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    [HarmonyPrefix]
    public static void BuffSystem_Spawn_Server_OnUpdate_Prefix(BuffSystem_Spawn_Server __instance)
    {
        var query = __instance._Query;
        var entities = query.ToEntityArray(Allocator.Temp);
        var lifeTimes = query.ToComponentDataArray<ProjectM.LifeTime>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[0];
            if (!EntityManager.TryGetComponentData<PrefabGUID>(entity, out var prefabGUID))
            {
                continue;
            }
            if (prefabGUID.Equals(WallopWarpersUtil.Buff_General_Phasing))
            {
                WallopWarpersUtil.ModifyBuffBeforeSpawn_DoNotImpairWaypointUse(entity);

                var lifeTime = lifeTimes[i];
                lifeTime.Duration = 10; // seconds
                EntityManager.SetComponentData(entity, lifeTime);

                // todo: don't remove phasing buff when interact with tp
                // that would involve the DestroyOnAbilityCast component probably
            }

            //LogUtil.LogDebug("------------------------");
            //DebugUtil.LogComponentTypes(entity);
            //DebugUtil.LogPrefabGuid(entity);
            //DebugUtil.LogBuffModificationFlagData(entity);
            //DebugUtil.LogLifeTime(entity);
            //DebugUtil.LogBuffCategory(entity);

            //if (EntityManager.TryGetComponentData<EntityCreator>(entity, out var entityCreator))
            //{
            //    LogUtil.LogInfo("creator --");
            //    DebugUtil.LogPrefabGuid(entityCreator.Creator._Entity);
            //    DebugUtil.LogComponentTypes(entityCreator.Creator._Entity);
            //}
        }
    }

}