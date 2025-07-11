using System;
using System.Collections.Generic;
using HarmonyLib;
using HookDOTS.API.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Systems;
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
        if (Plugin.PvPCombat_AllowWaygateUse.Value)
        {
            return;
        }

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
        if (Plugin.PvPCombat_AllowWaygateUse.Value)
        {
            return;
        }
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

                WallopWarpersUtil.InterruptCast3(entity, ev);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }
    }

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
            LogUtil.LogDebug($"TeleportBuffSpawnSystem | {DebugUtil.LookupPrefabName(entity)}");
        }
    }

    [HarmonyPatch(typeof(TeleportBuffSystem_Server), nameof(TeleportBuffSystem_Server.OnUpdate))]
    [HarmonyPrefix]
    public static void TeleportBuffSystem_Server_OnUpdate_Prefix(TeleportBuffSystem_Server __instance)
    {
        var query = __instance.__query_2122398833_1;
        var entities = query.ToEntityArray(Allocator.Temp);
        var buffs = query.ToComponentDataArray<Buff>(Allocator.Temp);
        var teleportBuffs = query.ToComponentDataArray<TeleportBuff>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[0];
            LogUtil.LogDebug($"TeleportBuffSystem_Server | {DebugUtil.LookupPrefabName(entity)}");
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
            if (prefabGUID.Equals(PrefabGuids.Buff_General_Phasing))
            {
                if (Plugin.SpawnProtection_AllowWaygateUse.Value)
                {
                    WallopWarpersUtil.ModifyBuffBeforeSpawn_DoNotImpairWaypointUse(entity);
                }

                var lifeTime = lifeTimes[i];
                lifeTime.Duration = Plugin.SpawnProtectionSeconds.Value;
                EntityManager.SetComponentData(entity, lifeTime);
            }
            else
            {
                LogUtil.LogDebug($"BuffSystem_Spawn_Server | {DebugUtil.LookupPrefabName(entity)}");
            }
        }
    }

}