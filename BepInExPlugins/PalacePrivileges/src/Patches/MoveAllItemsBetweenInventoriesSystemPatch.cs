using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;


[HarmonyPatch]
public unsafe class MoveAllItemsBetweenInventoriesSystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(MoveAllItemsBetweenInventoriesSystem), nameof(MoveAllItemsBetweenInventoriesSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void MoveAllItemsBetweenInventoriesSystem_OnUpdate_Prefix(MoveAllItemsBetweenInventoriesSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var moveAllItemsEvents = query.ToComponentDataArray<MoveAllItemsBetweenInventoriesEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            ProcessMoveAllItemsBetweenInventoriesEvent(entities[i], fromCharacters[i], moveAllItemsEvents[i], ref networkIdToEntityMap);
        }
    }

    private static void ProcessMoveAllItemsBetweenInventoriesEvent(
        Entity eventEntity,
        FromCharacter fromCharacter,
        MoveAllItemsBetweenInventoriesEvent moveAllItemsEvent,
        ref NetworkIdLookupMap networkIdToEntityMap
    )
    {
        if (!networkIdToEntityMap.TryGetValue(moveAllItemsEvent.FromInventory, out var fromInventory))
        {
            return;
        }

        if (!networkIdToEntityMap.TryGetValue(moveAllItemsEvent.ToInventory, out var toInventory))
        {
            return;
        }

        var isFromCastleHeartInventory = _entityManager.TryGetComponentData<CastleHeart>(fromInventory, out var fromCastleHeart);
        if (isFromCastleHeartInventory)
        {
            HandleMoveAllItems_FromCastleHeartInventory(
                eventEntity: eventEntity,
                fromCharacter: fromCharacter,
                fromInventory: fromInventory,
                toInventory: toInventory,
                castleHeartEntity: fromInventory,
                castleHeart: fromCastleHeart,
                moveItemEvent: moveAllItemsEvent
            );
            return;
        }
    }

    private static void HandleMoveAllItems_FromCastleHeartInventory(
        Entity eventEntity,
        FromCharacter fromCharacter,
        Entity fromInventory,
        Entity toInventory,
        Entity castleHeartEntity,
        CastleHeart castleHeart,
        MoveAllItemsBetweenInventoriesEvent moveItemEvent
    )
    {
        var character = fromCharacter.Character;
        var ruling = Core.RestrictionService.ValidateAction_CastleHeartRemoveFuel(character, castleHeartEntity);
        if (!ruling.IsAllowed)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
            _entityManager.DestroyEntity(eventEntity);
        }
    }

}