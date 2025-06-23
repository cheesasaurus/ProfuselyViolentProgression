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
            //DebugUtil.LogComponentTypes(dealDamageEvent.SpellSource);
            DebugUtil.LogHitColliderCast(dealDamageEvent.SpellSource);
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

}