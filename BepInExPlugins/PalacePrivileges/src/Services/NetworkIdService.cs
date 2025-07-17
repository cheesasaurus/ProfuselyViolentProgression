using ProfuselyViolentProgression.Core.Utilities;
using ProjectM.Network;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class NetworkIdService
{
    EntityQuery _queryNetworkIdServiceSingleton;
    EntityManager _entityManager;

    public NetworkIdService()
    {
        _entityManager = WorldUtil.Server.EntityManager;
        _queryNetworkIdServiceSingleton = _entityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<NetworkIdSystem.Singleton>(),
            },
            Options = EntityQueryOptions.IncludeSystems,
        });
    }

    /// <summary>
    /// Get a lookup map to find entities by their network id.
    /// </summary>
    /// <remarks>
    /// Do not cache this. component singletons are structs and i suspect
    /// 1) we are working with a copy.
    /// 2) the struct could be moved so we shouldn't store a pointer.
    /// </remarks>
    public NetworkIdLookupMap NetworkIdToEntityMap()
    {
        var singleton = _queryNetworkIdServiceSingleton.GetSingleton<NetworkIdSystem.Singleton>();
        return singleton.GetNetworkIdLookupRO();
    }

}