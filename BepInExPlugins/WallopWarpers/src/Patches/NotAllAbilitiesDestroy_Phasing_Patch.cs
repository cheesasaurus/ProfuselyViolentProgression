using System.Collections.Generic;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.WallopWarpers;


/// <summary>
/// Keeps the phasing buff if the ability casted is a waypoint interaction
/// </summary>
[HarmonyPatch]
public static class NotAllAbilitiesDestroy_Phasing_Patch
{
    public static EntityManager EntityManager => WorldUtil.Server.EntityManager;

    private static bool _queryAbilityCastStarted_cached = false;
    private static EntityQuery _queryAbilityCastStarted;
    private static EntityQuery QueryAbilityCastStarted {
        get
        {
            if (!_queryAbilityCastStarted_cached)
            {
                _queryAbilityCastStarted = EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                        ComponentType.ReadOnly<AbilityCastStartedEvent>(),
                    },
                });
            }
            return _queryAbilityCastStarted;
        }
    }

    private static bool _queryDestroyBuffsOnAbilityCast_cached = false;
    private static EntityQuery _queryDestroyBuffsOnAbilityCast;
    private static EntityQuery QueryDestroyBuffsOnAbilityCast {
        get
        {
            if (!_queryDestroyBuffsOnAbilityCast_cached)
            {
                _queryDestroyBuffsOnAbilityCast = EntityManager.CreateEntityQuery(new EntityQueryDesc()
                {
                    All = new ComponentType[] {
                        ComponentType.ReadOnly<Buff>(),
                        ComponentType.ReadWrite<DestroyOnAbilityCast>(),
                        ComponentType.ReadOnly<PrefabGUID>(),
                        ComponentType.ReadOnly<EntityOwner>(),
                    },
                });
            }
            return _queryDestroyBuffsOnAbilityCast;
        }
    }

    private static Dictionary<Entity, int> _tpSafetyBalance = new();

    [EcsSystemUpdatePrefix(typeof(DestroyOnAbilityCastSystem))]
    public static void DestroyOnAbilityCastSystem_Prefix()
    {
        if (!Plugin.SpawnProtection_AllowWaygateUse.Value)
        {
            return;
        }

        _tpSafetyBalance.Clear();
        
        var castStartedEvents = QueryAbilityCastStarted.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
        for (var i = 0; i < castStartedEvents.Length; i++)
        {
            var ev = castStartedEvents[i];
            if (EntityManager.TryGetComponentData<PrefabGUID>(ev.Ability, out var prefabGUID))
            {
                if (!_tpSafetyBalance.ContainsKey(ev.Character))
                {
                    _tpSafetyBalance.Add(ev.Character, 0);
                }
                var score = WallopWarpersUtil.IsUseWaypointCast(prefabGUID) ? 1 : -1;
                _tpSafetyBalance[ev.Character] += score;
            }
        }

        var queryDestroy = QueryDestroyBuffsOnAbilityCast;
        var entitiesDestroy = queryDestroy.ToEntityArray(Allocator.Temp);
        var destroyOnAbilityCasts = queryDestroy.ToComponentDataArray<DestroyOnAbilityCast>(Allocator.Temp);
        var prefabGuidsDestroy = queryDestroy.ToComponentDataArray<PrefabGUID>(Allocator.Temp);        
        var entityOwners = queryDestroy.ToComponentDataArray<EntityOwner>(Allocator.Temp);
        for (var i = 0; i < entitiesDestroy.Length; i++)
        {
            var entity = entitiesDestroy[0];
            var owner = entityOwners[0].Owner;
            if (prefabGuidsDestroy[i].Equals(PrefabGuids.Buff_General_Phasing))
            {
                if (!_tpSafetyBalance.ContainsKey(owner) || _tpSafetyBalance[owner] < 1)
                {
                    continue;
                }
                // set CastCount to zero, to prevent the buff from being destroyed
                var destroyOnAbilityCast = destroyOnAbilityCasts[i];
                destroyOnAbilityCast.CastCount = 0;
                EntityManager.SetComponentData(entity, destroyOnAbilityCast);
            }
        }
    }

}