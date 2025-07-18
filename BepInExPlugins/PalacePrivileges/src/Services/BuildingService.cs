using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using ProjectM.Network;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

/// <summary>
/// Knows things about building castles
/// </summary>
public class BuildingService
{
    private EntityManager _entityManager = WorldUtil.Server.EntityManager;


    


    public bool IsSiegeStructure(PrefabGUID prefabGUID)
    {
        // todo: implement
        return false;
    }

}