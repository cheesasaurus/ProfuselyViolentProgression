using BepInEx.Logging;
using ProfuselyViolentProgression.BoneBanditBrandHit.Services;
using ProfuselyViolentProgression.BoneBanditBrandHits.Services;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM.Scripting;

namespace ProfuselyViolentProgression.BoneBanditBrandHit;

public static class Core
{
    public static bool IsInitialized { get; private set; } = false;
    
    public static ServerScriptMapper ServerScriptMapper { get; private set; }
    public static ServerGameManager FetchServerGameManager() => ServerScriptMapper.GetServerGameManager(); // it's a struct, so don't cache it
    public static SCTService SCTService { get; private set; }
    public static NotificationService NotificationService { get; private set; }

    public static void Initialize(ManualLogSource log)
    {
        IsInitialized = true;

        var entityManager = WorldUtil.Server.EntityManager;
        ServerScriptMapper = WorldUtil.Server.GetExistingSystemManaged<ServerScriptMapper>();
        SCTService = new(ref entityManager);
        NotificationService = new(ref entityManager, SCTService);
    }

    public static void Dispose()
    {
        if (!IsInitialized)
        {
            return;
        }
        IsInitialized = false;
    }

}