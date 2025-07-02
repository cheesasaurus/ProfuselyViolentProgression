using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.CollisionCorrection;


[HarmonyPatch]
public unsafe class Patches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;
    private static EntityQuery Query_;

    [EcsSystemUpdatePostfix(typeof(HandleGameplayEventsRecursiveSystem))]
    public static void CheckDealDamageEvents()
    {
        if (Query_ == default)
        {
            Query_ = EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<DealDamageEvent>(),
                },
            });
        }

        var dealDamageEvents = Query_.ToComponentDataArray<DealDamageEvent>(Allocator.Temp);
        foreach (var dealDamageEvent in dealDamageEvents)
        {
            var entity = dealDamageEvent.SpellSource;
            DebugUtil.LogPrefabGuid(entity);
            //DebugUtil.LogComponentTypes(entity);
            //DebugUtil.LogHitTriggers(entity);
            //DebugUtil.LogHitColliderCast(entity);
            //DebugUtil.LogTriggerHitConsume(entity);

        }
    }



    //[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    //[HarmonyPrefix]
    //public static void BuffSpawnPrefix(BuffSystem_Spawn_Server __instance)
    //{
    //    var entities = __instance._Query.ToEntityArray(Allocator.Temp);
    //    var buffs = __instance._Query.ToComponentDataArray<Buff>(Allocator.Temp);
    //
    //    for (var i = 0; i < entities.Length; i++)
    //    {
    //        //FixDashAttackTriggersUtil.BuffWillBeSpawned(entities[i], buffs[i].Target);
    //    }
    //}


    [HarmonyPatch(typeof(HitCastColliderSystem_OnUpdate), nameof(HitCastColliderSystem_OnUpdate.OnUpdate))]
    [HarmonyPrefix]
    public static void HitCastColliderSystem_OnUpdate_Prefix(HitCastColliderSystem_OnUpdate __instance)
    {
        var query = __instance.__query_911162766_0;

        var entities = query.ToEntityArray(Allocator.Temp);
        for (var i = 0; i < entities.Length; i++)
        {
            var entity = entities[i];
            DebugUtil.LogPrefabGuid(entity);
            //DebugUtil.LogComponentTypes(entity);
            //DebugUtil.LogCreateGameplayEventsOnHit(entity);
            DebugUtil.LogGameplayEventListeners(entity);
            //DebugUtil.LogPlayImpactOnGameplayEvent(entity);
            //DebugUtil.LogTriggerHitConsume(entity);
            //DebugUtil.LogDealDamageOnGameplayEvent(entity);
            //DebugUtil.LogAbilityProjectileFanOnGameplayEvent_DataServer(entity);
            //DebugUtil.LogProjectilDestroyData(entity);

            //DoSomething(entity);
        }
    }

    private static void DoSomething(Entity entity)
    {
        if (!EntityManager.HasBuffer<HitColliderCast>(entity))
        {
            return;
        }
        var buffer = EntityManager.GetBuffer<HitColliderCast>(entity);

        for (var i = 0; i < buffer.Length; i++)
        {
            var hcc = buffer[i];
            //hcc.CanHitThroughBlockSpellCollision = false;
            //hcc.PrimaryTargets_Count = 2;
            //hcc.SecondaryTargets_Count = 0;
            buffer[i] = hcc;
        }
        DebugUtil.LogHitColliderCast(entity);
    }

    private static void DoSomething2(Entity entity)
    {
        if (!EntityManager.TryGetComponentRW<ProjectileDestroyData>(entity, out var pdd))
        {
            return;
        }
        pdd.ValueRW.HasHitTarget = false;
    }

}