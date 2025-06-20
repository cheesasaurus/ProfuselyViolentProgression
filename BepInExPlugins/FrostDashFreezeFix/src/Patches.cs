using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Systems;
using ProjectM.UI;
using ProjectM.WeaponCoating;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.FrostDashFreezeFix;


[HarmonyPatch]
public unsafe class MiscPatches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;

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
        // todo: don't recreate the query every call
        var query = EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<DealDamageEvent>(),
            },
        });

        var events = query.ToEntityArray(Allocator.Temp);
        foreach (var eventEntity in events)
        {
            var dealDamage = EntityManager.GetComponentData<DealDamageEvent>(eventEntity);
            FreezeFixUtil.EntityGotHitWithDamage(dealDamage.Target);
        }
    }


    [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
    [HarmonyPrefix]
    public static void Prefix(BuffSystem_Spawn_Server __instance)
    {
        var events = __instance._Query.ToEntityArray(Allocator.Temp);
        foreach (var entity in events)
        {
            FreezeFixUtil.BuffWillBeSpawned(entity);
        }
        FreezeFixUtil.ModifyBadFrostDashes();
    }

}