using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;


[HarmonyPatch]
public unsafe class MoveItemBetweenInventoriesSystemPatch
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(MoveItemBetweenInventoriesSystem), nameof(MoveItemBetweenInventoriesSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void MoveItemBetweenInventoriesSystem_OnUpdate_Prefix(MoveItemBetweenInventoriesSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._MoveItemBetweenInventoriesEventQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var moveItemEvents = query.ToComponentDataArray<MoveItemBetweenInventoriesEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            ProcessMoveItemBetweenInventoriesEvent(entities[i], fromCharacters[i], moveItemEvents[i], ref networkIdToEntityMap);
        }
    }

    private static void ProcessMoveItemBetweenInventoriesEvent(
        Entity eventEntity,
        FromCharacter fromCharacter,
        MoveItemBetweenInventoriesEvent moveItemEvent,
        ref NetworkIdLookupMap networkIdToEntityMap
    )
    {
        if (!networkIdToEntityMap.TryGetValue(moveItemEvent.FromInventory, out var fromInventory))
        {
            return;
        }

        if (!networkIdToEntityMap.TryGetValue(moveItemEvent.ToInventory, out var toInventory))
        {
            return;
        }

        var isFromServantInventory = _entityManager.TryGetComponentData<ServantConnectedCoffin>(fromInventory, out var fromServant_ConnectedCoffin);
        if (isFromServantInventory)
        {
            HandleMoveItem_FromServantInventory(
                eventEntity: eventEntity,
                fromCharacter: fromCharacter,
                fromInventory: fromInventory,
                toInventory: toInventory,
                servantCoffin: fromServant_ConnectedCoffin.CoffinEntity._Entity,
                moveItemEvent: moveItemEvent
            );
            return;
        }

        var isToServantInventory = _entityManager.TryGetComponentData<ServantConnectedCoffin>(toInventory, out var toServant_ConnectedCoffin);
        if (isToServantInventory)
        {
            HandleMoveItem_ToServantInventory(
                eventEntity: eventEntity,
                fromCharacter: fromCharacter,
                fromInventory: fromInventory,
                toInventory: toInventory,
                servantCoffin: toServant_ConnectedCoffin.CoffinEntity._Entity,
                moveItemEvent: moveItemEvent
            );
            return;
        }

        var isFromCastleHeartInventory = _entityManager.TryGetComponentData<CastleHeart>(fromInventory, out var fromCastleHeart);
        if (isFromCastleHeartInventory)
        {
            HandleMoveItem_FromCastleHeartInventory(
                eventEntity: eventEntity,
                fromCharacter: fromCharacter,
                fromInventory: fromInventory,
                toInventory: toInventory,
                castleHeartEntity: fromInventory,
                castleHeart: fromCastleHeart,
                moveItemEvent: moveItemEvent
            );
            return;
        }
    }

    private static void HandleMoveItem_FromServantInventory(
        Entity eventEntity,
        FromCharacter fromCharacter,
        Entity fromInventory,
        Entity toInventory,
        Entity servantCoffin,
        MoveItemBetweenInventoriesEvent moveItemEvent
    )
    {
        // Currently no restrictions that need to be handled here.
        // LogUtil.LogDebug("moving FROM servant inventory.");
    }

    private static void HandleMoveItem_ToServantInventory(
        Entity eventEntity,
        FromCharacter fromCharacter,
        Entity fromInventory,
        Entity toInventory,
        Entity servantCoffin,
        MoveItemBetweenInventoriesEvent moveItemEvent
    )
    {
        // Currently no restrictions that need to be handled here.
        // LogUtil.LogDebug("moving TO servant inventory.");

        // This covers moving items to a servant's inventory.
        // The item could end up getting auto equipped, which we handle elsewhere, via other patches.
    }

    private static void HandleMoveItem_FromCastleHeartInventory(
        Entity eventEntity,
        FromCharacter fromCharacter,
        Entity fromInventory,
        Entity toInventory,
        Entity castleHeartEntity,
        CastleHeart castleHeart,
        MoveItemBetweenInventoriesEvent moveItemEvent
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