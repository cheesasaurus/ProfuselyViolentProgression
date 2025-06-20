using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.FrostDashFreezeFix;


[HarmonyPatch]
public unsafe class Patches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;
    private static EntityQuery Query_DealDamageEvent;

    [EcsSystemUpdatePrefix(typeof(RecursiveGroup), onlyWhenSystemRuns: false)]
    public static void UpdateTickCount()
    {
        FreezeFixUtil.NewTickStarted();

    }

    [HarmonyPatch(typeof(RecursiveGroup), nameof(RecursiveGroup.DoRecursiveUpdate))]
    [HarmonyPrefix]
    public static void TrackRecursiveUpdates()
    {
        FreezeFixUtil.RecursiveGroupUpdateStarting();
    }

    [EcsSystemUpdatePostfix(typeof(HandleGameplayEventsRecursiveSystem))]
    public static void CheckDealDamageEvents()
    {
        if (Query_DealDamageEvent == default)
        {
            Query_DealDamageEvent = EntityManager.CreateEntityQuery(new EntityQueryDesc()
            {
                All = new ComponentType[] {
                    ComponentType.ReadOnly<DealDamageEvent>(),
                },
            });
        }

        var dealDamageEvents = Query_DealDamageEvent.ToComponentDataArray<DealDamageEvent>(Allocator.Temp);
        foreach (var dealDamageEvent in dealDamageEvents)
        {
            FreezeFixUtil.EntityGotHitWithDamage(dealDamageEvent.Target);
        }
    }


    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    [HarmonyPrefix]
    public static void Prefix(BuffSystem_Spawn_Server __instance)
    {
        var entities = __instance._Query.ToEntityArray(Allocator.Temp);
        var buffs = __instance._Query.ToComponentDataArray<Buff>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            FreezeFixUtil.BuffWillBeSpawned(entities[i], buffs[i].Target);
        }
    }

}