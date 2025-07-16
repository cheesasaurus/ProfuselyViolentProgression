using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.CastleBuilding;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

// knows about doors. decides what privileges are acceptable for opening a door
public class CastleDoorService
{

    private EntityManager _entityManager = WorldUtil.Server.EntityManager;
    private CastleService _castleService;
    private Dictionary<PrefabGUID, DoorPrivs> _privsByPrefabGuid = [];

    public CastleDoorService(CastleService castleService)
    {
        _castleService = castleService;
        InitPrivsByPrefabGUID();
    }

    public bool TryGetDoorModel(Entity doorEntity, out CastleDoorModel doorModel)
    {
        doorModel = default;

        if (!_entityManager.TryGetComponentData<PrefabGUID>(doorEntity, out var prefabGUID))
        {
            LogUtil.LogWarning("door: no PrefabGUID"); // todo: toggle for this kind of logging
            return false;
        }

        if (!_entityManager.TryGetComponentData<Door>(doorEntity, out var door))
        {
            LogUtil.LogWarning("door: no Door");
            return false;
        }

        if (!_entityManager.TryGetComponentData<Team>(doorEntity, out var team))
        {
            LogUtil.LogWarning("door: no Team");
            return false;
        }

        if (!_castleService.TryGetCastleModel_ForConnectedEntity(doorEntity, out var castleModel))
        {
            LogUtil.LogWarning("door: no CastleModel");
            return false;
        }

        doorModel = new CastleDoorModel
        {
            PrefabGUID = prefabGUID,
            Castle = castleModel,
            Team = team,
            AcceptablePrivilegesToOpen = new CastlePrivileges
            {
                Door = AssociatedPrivileges(door, prefabGUID),
            },
        };
        return true;
    }

    public DoorPrivs AssociatedPrivileges(Door door, PrefabGUID prefabGUID)
    {
        LogUtil.LogDebug(DebugUtil.LookupPrefabName(prefabGUID));

        var privs = door.CanBeOpenedByServant ? DoorPrivs.NotServantLocked : DoorPrivs.ServantLocked;

        // todo: check things like width, texture, etc

        return privs;
    }

    private void InitPrivsByPrefabGUID()
    {
        // todo: implement
    }
}
