using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.FrostDashFreezeFix;

public static class FreezeFixUtil
{
    public static int TickCount = 0;
    public static int RecursiveGroupPassesThisTick = 0;
    public static string RecursiveGroupTickStamp { get => $"{TickCount}-{RecursiveGroupPassesThisTick}"; }

    public static HashSet<Entity> HitWhileNotChilledThisTick = new();

    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;

    static PrefabGUID NullPrefabGUID = new PrefabGUID(0);
    static PrefabGUID Frost_Vampire_Buff_Chill = new PrefabGUID(27300215);
    static PrefabGUID AB_Vampire_VeilOfFrost_TriggerBonusEffects = new PrefabGUID(-1688602321);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack = new PrefabGUID(-292495274);

    public static void NewTickStarted()
    {
        TickCount++;
        RecursiveGroupPassesThisTick = 0;
        HitWhileNotChilledThisTick.Clear();
    }

    public static void RecursiveGroupUpdateStarting()
    {
        RecursiveGroupPassesThisTick++;
    }    

    public static void EntityGotHitWithDamage(Entity entity)
    {
        if (!IsEntityChilled(entity))
        {
            HitWhileNotChilledThisTick.Add(entity);
        }
    }

    public static void BuffWillBeSpawned(Entity entity, Entity entityToBuff)
    {
        if (HitWhileNotChilledThisTick.Contains(entityToBuff) && IsFrostDashTriggerBuff(entity))
        {
            RemoveFrostDashFreezeSpellMods(entity);
        }
    }

    public static bool IsFrostDashTriggerBuff(Entity entity)
    {
        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return false;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);
        return prefabGUID.Equals(AB_Vampire_VeilOfFrost_TriggerBonusEffects);
    }

    public static bool IsEntityChilled(Entity entity)
    {
        if (EntityManager.HasBuffer<BuffBuffer>(entity))
        {
            var targetsBuffs = EntityManager.GetBuffer<BuffBuffer>(entity);
            foreach (var buff in targetsBuffs)
            {
                if (buff.PrefabGuid.Equals(Frost_Vampire_Buff_Chill))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static void RemoveFrostDashFreezeSpellMods(Entity entity)
    {
        if (!EntityManager.HasBuffer<SpellModSetComponent>(entity))
        {
            return;
        }
        var smsc = EntityManager.GetComponentData<SpellModSetComponent>(entity);
        var sm = smsc.SpellMods;

        if (sm.Mod0.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod0.Id = NullPrefabGUID;
        }
        if (sm.Mod1.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod1.Id = NullPrefabGUID;
        }
        if (sm.Mod2.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod2.Id = NullPrefabGUID;
        }
        if (sm.Mod3.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod3.Id = NullPrefabGUID;
        }
        if (sm.Mod4.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod4.Id = NullPrefabGUID;
        }
        if (sm.Mod5.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod5.Id = NullPrefabGUID;
        }
        if (sm.Mod6.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod6.Id = NullPrefabGUID;
        }
        if (sm.Mod7.Id.Equals(SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack))
        {
            sm.Mod7.Id = NullPrefabGUID;
        }
        smsc.SpellMods = sm;
        EntityManager.SetComponentData(entity, smsc);
    }

}
