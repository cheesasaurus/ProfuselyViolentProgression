using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;


[HarmonyPatch]
public unsafe class MoveAllItemsBetweenInventoriesV2SystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(MoveAllItemsBetweenInventoriesV2System), nameof(MoveAllItemsBetweenInventoriesV2System.OnUpdate))]
    [HarmonyPrefix]
    public static void MoveAllItemsBetweenInventoriesV2System_OnUpdate_Prefix(MoveAllItemsBetweenInventoriesV2System __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._EventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var moveAllItemsEvents = query.ToComponentDataArray<MoveAllItemsBetweenInventoriesEventV2>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            ProcessMoveAllItemsBetweenInventoriesV2Event(entities[i], fromCharacters[i], moveAllItemsEvents[i], ref networkIdToEntityMap);
        }
    }

    private static void ProcessMoveAllItemsBetweenInventoriesV2Event(
        Entity eventEntity,
        FromCharacter fromCharacter,
        MoveAllItemsBetweenInventoriesEventV2 moveAllItemsEvent,
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
        MoveAllItemsBetweenInventoriesEventV2 moveItemEvent
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