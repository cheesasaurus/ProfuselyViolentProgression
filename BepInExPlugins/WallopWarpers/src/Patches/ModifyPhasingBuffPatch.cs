using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.WallopWarpers.Patches;

[HarmonyPatch]
public static class ModifyPhasingBuffPatch
{
    public static EntityManager EntityManager => WorldUtil.Server.EntityManager;

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
        }
    }

}