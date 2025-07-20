using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class ServantCoffinPatches
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(ServantCoffinstationActionSystem), nameof(ServantCoffinstationActionSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void ServantCoffinstationActionSystem_OnUpdate_Prefix(ServantCoffinstationActionSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var servantCoffinActionEvents = query.ToComponentDataArray<ServantCoffinActionEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = servantCoffinActionEvents[i];

            if (!networkIdToEntityMap.TryGetValue(ev.Workstation, out var coffin))
            {
                continue;
            }

            var actingCharacter = fromCharacters[i].Character;

            var ruling = Core.RestrictionService.ValidateAction_AtCoffin(actingCharacter, coffin, ev);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(actingCharacter, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }
        }
    }

    [HarmonyPatch(typeof(ServantCoffinstationUpdateSystem), nameof(ServantCoffinstationUpdateSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void ServantCoffinstationUpdateSystem_OnUpdate_Prefix(ServantCoffinstationUpdateSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._ChangeServantNameEventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var changeServantNameEvents = query.ToComponentDataArray<ChangeServantNameEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = changeServantNameEvents[i];

            if (!networkIdToEntityMap.TryGetValue(ev.Workstation, out var coffin))
            {
                continue;
            }

            var actingCharacter = fromCharacters[i].Character;

            var ruling = Core.RestrictionService.ValidateAction_ServantRename(actingCharacter, coffin);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(actingCharacter, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }
        }
    }

}