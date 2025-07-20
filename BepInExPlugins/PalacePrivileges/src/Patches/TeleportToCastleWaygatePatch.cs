using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public static class TeleportToCastleWaygatePatch
{
    private static EntityManager _entityManager => WorldUtil.Server.EntityManager;

    [HarmonyPatch(typeof(TeleportationRequestSystem), nameof(TeleportationRequestSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void TeleportationRequestSystem_OnUpdate_Prefix(TeleportationRequestSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._TeleportRequestQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<TeleportationRequest>(Allocator.Temp);
        
        for (var i = 0; i < entities.Length; i++)
        {
            var ev = events[i];

            if (!_entityManager.HasComponent<CastleWaypoint>(ev.ToTarget))
            {
                continue;
            }

            var character = ev.PlayerEntity;
            var ruling = Core.RestrictionService.ValidateAction_WaygateIn(character, ev.ToTarget);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }
        }
    }

}