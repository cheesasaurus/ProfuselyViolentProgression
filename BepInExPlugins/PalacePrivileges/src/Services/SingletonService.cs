using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;


/// <remarks>
/// Singleton components (and things derived from them) should probably not be cached. They're structs, and I suspect:
/// 1) we are working with a copy.
/// 2) the struct could be moved so we shouldn't store a pointer.
/// 
/// Therefore, don't fetch them in a loop, because you'll be making a big copy over and over.
/// Instead, fetch them before starting a batch of stuff, and pass refs.
/// </remarks>
public class SingletonService
{
    EntityManager _entityManager;
    EntityQuery _queryNetworkIdSystemSingleton;
    EntityQuery _queryMapZoneCollectionSingleton;

    public SingletonService()
    {
        _entityManager = WorldUtil.Server.EntityManager;

        _queryNetworkIdSystemSingleton = _entityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<NetworkIdSystem.Singleton>(),
            },
            Options = EntityQueryOptions.IncludeSystems,
        });

        _queryMapZoneCollectionSingleton = _entityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<MapZoneCollection>(),
            },
            Options = EntityQueryOptions.IncludeSystems,
        });
    }

    public MapZoneCollection FetchMapZoneCollection()
    {
        return _queryMapZoneCollectionSingleton.GetSingleton<MapZoneCollection>();
    }
    
    public NetworkIdLookupMap FetchNetworkIdToEntityMap()
    {
        var singleton = _queryNetworkIdSystemSingleton.GetSingleton<NetworkIdSystem.Singleton>();
        return singleton.GetNetworkIdLookupRO();
    }

}