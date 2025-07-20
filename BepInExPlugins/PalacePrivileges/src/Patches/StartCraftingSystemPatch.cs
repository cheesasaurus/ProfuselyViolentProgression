using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;


/// <remarks>
/// StartCraftingSystem processes StartCraftItemEvent.
/// Most of the actions at a prison cell are actually crafting recipes.
/// (Everything except subdue and kill)
/// </remarks>
[HarmonyPatch]
public unsafe class StartCraftingSystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(StartCraftingSystem), nameof(StartCraftingSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void SomePatchThing(StartCraftingSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._StartCraftItemEventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var startCraftItemEvents = query.ToComponentDataArray<StartCraftItemEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = startCraftItemEvents[i];
            var character = fromCharacters[i].Character;
            if (!networkIdToEntityMap.TryGetValue(ev.Workstation, out var workstation))
            {
                continue;
            }

            if (_entityManager.HasComponent<Prisonstation>(workstation))
            {
                HandlePrisonCraft(entities[i], character, workstation, ev.RecipeId);
            }
            else
            {
                HandleGeneralCraft(entities[i], character, workstation, ev.RecipeId);
            }
        }
    }

    private static void HandlePrisonCraft(Entity eventEntity, Entity character, Entity workstation, PrefabGUID recipePrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_PrisonerFeeding(character, workstation, recipePrefabGUID);
        EnforceRuling(eventEntity, character, ruling);
    }

    private static void HandleGeneralCraft(Entity eventEntity, Entity character, Entity workstation, PrefabGUID recipePrefabGUID)
    {
        var ruling = Core.RestrictionService.ValidateAction_CraftItem(character, workstation, recipePrefabGUID);
        EnforceRuling(eventEntity, character, ruling);
    }
    
    private static void EnforceRuling(Entity eventEntity, Entity character, CastleActionRuling ruling)
    {
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
            _entityManager.DestroyEntity(eventEntity);
        }
    }

}