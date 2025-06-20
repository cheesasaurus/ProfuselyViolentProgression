using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.SurgeSuppressor;

[HarmonyPatch]
public unsafe class Patches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;
    private static EntityQuery Query_DealDamageEvent;

    [EcsSystemUpdatePrefix(typeof(RecursiveGroup), onlyWhenSystemRuns: false)]
    public static void TickStartPrefix()
    {
        SurgeSuppressorUtil.NewTickStarted();
    }

    [HarmonyPatch(typeof(RecursiveGroup), nameof(RecursiveGroup.DoRecursiveUpdate))]
    [HarmonyPrefix]
    public static void TrackRecursiveUpdates()
    {
        SurgeSuppressorUtil.RecursiveGroupUpdateStarting();
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

        var entities = Query_DealDamageEvent.ToEntityArray(Allocator.Temp);
        var dealDamageEvents = Query_DealDamageEvent.ToComponentDataArray<DealDamageEvent>(Allocator.Temp);
        for (var i = 0; i < entities.Length; i++)
        {
            SurgeSuppressorUtil.DamageWouldBeDealt(entities[i], dealDamageEvents[i]);
        }
    }

}