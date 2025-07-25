using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class CastleHeartEventSystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(CastleHeartEventSystem), nameof(CastleHeartEventSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void CastleHeartEventSystem_OnUpdate_Prefix(CastleHeartEventSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._CastleHeartInteractEventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var heartEvents = query.ToComponentDataArray<CastleHeartInteractEvent>(Allocator.Temp);

        var networkIdLookup = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var heartEvent = heartEvents[i];
            if (!networkIdLookup.TryGetValue(heartEvent.CastleHeart, out var castleHeart))
            {
                continue;
            }

            switch (heartEvent.EventType)
            {
                case CastleHeartInteractEventType.Abandon:
                    HandleAbandon(entities[i], fromCharacters[i], castleHeart);
                    break;

                case CastleHeartInteractEventType.Expose:
                    HandleExpose(entities[i], fromCharacters[i], castleHeart);
                    break;

                case CastleHeartInteractEventType.Raid:
                    HandleRaid(entities[i], fromCharacters[i], castleHeart);
                    break;

            }
        }

    }

    private static void HandleAbandon(Entity eventEntity, FromCharacter fromCharacter, Entity castleHeart)
    {
        var character = fromCharacter.Character;
        var ruling = Core.RestrictionService.ValidateAction_CastleHeartAbandon(character, castleHeart);
        EnforceRuling(eventEntity, character, ref ruling);
    }

    private static void HandleExpose(Entity eventEntity, FromCharacter fromCharacter, Entity castleHeart)
    {
        var character = fromCharacter.Character;
        var ruling = Core.RestrictionService.ValidateAction_CastleHeartExpose(character, castleHeart);
        EnforceRuling(eventEntity, character, ref ruling);
    }

    private static void HandleRaid(Entity eventEntity, FromCharacter fromCharacter, Entity castleHeart)
    {
        var character = fromCharacter.Character;
        var ruling = Core.RestrictionService.ValidateAction_CastleHeartDisableDefense(character, castleHeart);
        EnforceRuling(eventEntity, character, ref ruling);
    }

    private static void EnforceRuling(Entity eventEntity, Entity character, ref CastleActionRuling ruling)
    {
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
            _entityManager.DestroyEntity(eventEntity);
        }
    }

}