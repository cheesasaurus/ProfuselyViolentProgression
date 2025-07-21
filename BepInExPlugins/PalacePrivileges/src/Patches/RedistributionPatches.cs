using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Contest.Arena;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;



[HarmonyPatch]
public unsafe class RedistributionPatches
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;

    private static EntityQuery _queryRouteChangedEvents = _entityManager.CreateEntityQuery(new EntityQueryDesc()
    {
        All = new ComponentType[] {
            ComponentType.ReadOnly<FromCharacter>(),
            ComponentType.ReadOnly<InventoryRouteChangedEvent>(),
        },
    });

    private static EntityQuery _queryClearEvents = _entityManager.CreateEntityQuery(new EntityQueryDesc()
    {
        All = new ComponentType[] {
            ComponentType.ReadOnly<FromCharacter>(),
            ComponentType.ReadOnly<InventoryRouteClearEvent>(),
        },
    });

    private static EntityQuery _querySetAutoTransferEvents = _entityManager.CreateEntityQuery(new EntityQueryDesc()
    {
        All = new ComponentType[] {
            ComponentType.ReadOnly<FromCharacter>(),
            ComponentType.ReadOnly<InventoryRouteSetAutoTransferEvent>(),
        },
    });

    private static EntityQuery _queryQuickSendEvents = _entityManager.CreateEntityQuery(new EntityQueryDesc()
    {
        All = new ComponentType[] {
            ComponentType.ReadOnly<FromCharacter>(),
            ComponentType.ReadOnly<InventoryRouteTransferEvent>(),
        },
    });

    /// <remarks>
    /// InventoryRouteEventSystem handles changes to a single route:
    /// creating (connecting), deleting (disconnecting), reordering,
    /// and something we don't care about (counting?)
    /// </remarks>
    [EcsSystemUpdatePrefix(typeof(InventoryRouteEventSystem))]
    public static void InventoryRouteEventSystem_OnUpdate_Prefix()
    {
        var query = _queryRouteChangedEvents;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var routeClearEvents = query.ToComponentDataArray<InventoryRouteChangedEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = routeClearEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.FromEntity, out var station))
            {
                continue;
            }

            switch (ev.Type)
            {
                case InventoryRouteChangedEvent.EventType.Connect:
                    HandleRouteConnect(entities[i], character, station);
                    break;

                case InventoryRouteChangedEvent.EventType.Disconnect:
                    HandleRouteDisconnect(entities[i], character, station);
                    break;

                case InventoryRouteChangedEvent.EventType.SetRouteBufferIndex:
                    HandleRouteReorder(entities[i], character, station);
                    break;
            }
        }
    }

    private static void HandleRouteConnect(Entity eventEntity, Entity character, Entity station)
    {
        var ruling = Core.RestrictionService.ValidateAction_RedistributionRouteAdd(character, station);
        EnforceRuling(eventEntity, character, ref ruling);
    }

    private static void HandleRouteDisconnect(Entity eventEntity, Entity character, Entity station)
    {
        var ruling = Core.RestrictionService.ValidateAction_RedistributionRouteRemove(character, station);
        EnforceRuling(eventEntity, character, ref ruling);
    }

    private static void HandleRouteReorder(Entity eventEntity, Entity character, Entity station)
    {
        var ruling = Core.RestrictionService.ValidateAction_RedistributionRouteReorder(character, station);
        EnforceRuling(eventEntity, character, ref ruling);
    }

    /// <remarks>
    /// InventoryRouteStationEventSystem handles the one event coming from the routing station ("redistribution engine"):
    /// Clearing all routes.
    /// </remarks>
    [EcsSystemUpdatePrefix(typeof(InventoryRouteStationEventSystem))]
    public static void InventoryRouteStationEventSystem_OnUpdate_Prefix()
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = _queryClearEvents;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var routeClearEvents = query.ToComponentDataArray<InventoryRouteClearEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = routeClearEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.TargetStation, out var station))
            {
                continue;
            }

            var ruling = Core.RestrictionService.ValidateAction_RedistributionClearRoutes(character, station);
            EnforceRuling(entities[i], character, ref ruling);
        }
    }

    /// <remarks>
    /// InventoryRouteSetAutoTransferEventSystem toggles auto send
    /// </remarks>
    [EcsSystemUpdatePrefix(typeof(InventoryRouteSetAutoTransferEventSystem))]
    public static void InventoryRouteSetAutoTransferEventSystem_OnUpdate_Prefix()
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = _querySetAutoTransferEvents;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var toggleEvents = query.ToComponentDataArray<InventoryRouteSetAutoTransferEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = toggleEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.Target, out var station))
            {
                continue;
            }

            var ruling = Core.RestrictionService.ValidateAction_RedistributionToggleAutoSend(character, station);
            EnforceRuling(entities[i], character, ref ruling);
        }
    }

    /// <remarks>
    /// InventoryRouteTransferEventSystem handles manually transferring via "Quick Send"
    /// </remarks>
    [EcsSystemUpdatePrefix(typeof(InventoryRouteTransferEventSystem))]
    public static void InventoryRouteTransferEventSystem_OnUpdate_Prefix()
    {        
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = _queryQuickSendEvents;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var manualTransferEvents = query.ToComponentDataArray<InventoryRouteTransferEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.SingletonService.FetchNetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = manualTransferEvents[i];
            var character = fromCharacters[i].Character;

            if (!networkIdToEntityMap.TryGetValue(ev.FromEntity, out var station))
            {
                continue;
            }

            var ruling = Core.RestrictionService.ValidateAction_RedistributionQuickSend(character, station);
            EnforceRuling(entities[i], character, ref ruling);
        }
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