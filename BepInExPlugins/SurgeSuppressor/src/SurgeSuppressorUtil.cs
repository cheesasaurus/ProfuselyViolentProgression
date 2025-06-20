using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.SurgeSuppressor;

public static class SurgeSuppressorUtil
{
    public static int TickCount = 0;
    public static int RecursiveGroupPassesThisTick = 0;
    public static string RecursiveGroupTickStamp { get => $"{TickCount}-{RecursiveGroupPassesThisTick}"; }
    private static MultiThrottle<Entity> ShockThrottle = new(milliseconds: 250);
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;
    public static PrefabGUID Storm_Vampire_Buff_Static = new PrefabGUID(-1576512627);
    private static bool OnlyProtectPlayers = true;

    public static void NewTickStarted()
    {
        TickCount++;
        RecursiveGroupPassesThisTick = 0;
        ShockThrottle.Prune();
    }

    public static void RecursiveGroupUpdateStarting()
    {
        RecursiveGroupPassesThisTick++;
    }

    public static void SetSettings(bool onlyProtectPlayers, int throttleIntervalMilliseconds)
    {
        OnlyProtectPlayers = onlyProtectPlayers;
        ShockThrottle.ChangeInterval(milliseconds: throttleIntervalMilliseconds);
    }

    public static void DamageWouldBeDealt(Entity entity, DealDamageEvent dealDamageEvent)
    {
        if (OnlyProtectPlayers && !IsPlayerCharacter(dealDamageEvent.Target))
        {
            return;
        }

        if (IsDamageSourcedFromStaticShock(dealDamageEvent) && ShockThrottle.CheckAndTrigger(dealDamageEvent.Target))
        {
            //LogUtil.LogWarning("Cancelling static shock");
            EntityManager.DestroyEntity(entity);
        }
    }

    public static bool IsDamageSourcedFromStaticShock(DealDamageEvent dealDamageEvent)
    {
        if (!EntityManager.HasComponent<PrefabGUID>(dealDamageEvent.SpellSource))
        {
            return false;
        }
        var prefabGuid = EntityManager.GetComponentData<PrefabGUID>(dealDamageEvent.SpellSource);
        return prefabGuid.Equals(Storm_Vampire_Buff_Static);
    }

    public static bool IsPlayerCharacter(Entity entity)
    {
        return EntityManager.HasComponent<PlayerCharacter>(entity);
    }

}

