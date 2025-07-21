using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;


[HarmonyPatch]
public unsafe class ToggleRefiningSystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;

    [HarmonyPatch(typeof(ToggleRefiningRecipeSystem), nameof(ToggleRefiningRecipeSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void ToggleRefiningRecipeSystem_OnUpdate_Prefix(ToggleRefiningRecipeSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var toggleEvents = query.ToComponentDataArray<ToggleRefiningRecipeEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = toggleEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.RefinementStation, out var station))
            {
                continue;
            }

            var ruling = Core.RestrictionService.ValidateAction_ToggleRefinementRecipe(character, station, ev.RecipeId);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }

        }
    }

    // ToggleRefiningSystem only applies to the mist brazier?
    // we don't need to restrict that.
    /*
    [HarmonyPatch(typeof(ToggleRefiningSystem), nameof(ToggleRefiningSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void ToggleRefiningSystem_OnUpdate_Prefix(ToggleRefiningSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var toggleEvents = query.ToComponentDataArray<ToggleRefiningEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = toggleEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.Refinementstation, out var station))
            {
                continue;
            }

            var ruling = Core.RestrictionService.ValidateAction_ToggleRefinement(character, station);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }

        }
    }
    */

}