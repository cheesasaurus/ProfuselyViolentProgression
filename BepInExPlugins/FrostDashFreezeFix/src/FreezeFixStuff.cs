using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    public static HashSet<Entity> HitWhileNotChilledThisTick = new();
    public static Dictionary<Entity, Entity> FrostDashProcThisTick = new(); // todo: could be multiple events per victim 

    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;


    static PrefabGUID NullPrefabGUID = new PrefabGUID(0);
    static PrefabGUID Frost_Vampire_Buff_Chill = new PrefabGUID(27300215);
    static PrefabGUID AB_Vampire_VeilOfFrost_TriggerBonusEffects = new PrefabGUID(-1688602321);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack = new PrefabGUID(-292495274);

    public static void NewTickStarted()
    {
        TickCount++;
        RecursiveGroupPassesThisTick = 0;
        FrostDashProcThisTick.Clear();
        HitWhileNotChilledThisTick.Clear();
    }

    public static void RecursiveGroupUpdateStarting()
    {
        RecursiveGroupPassesThisTick++;
    }

    public static string RecursiveGroupTickStamp
    {
        get => $"{TickCount}-{RecursiveGroupPassesThisTick}";
    }

    public static void EntityGotHitWithDamage(Entity entity)
    {
        if (!IsEntityChilled(entity))
        {
            HitWhileNotChilledThisTick.Add(entity);
        }
    }

    public static void BuffWillBeSpawned(Entity entity)
    {
        if (!TryGetBuffTarget(entity, out var entityToBuff))
        {
            return;
        }

        if (IsFrostDashTriggerBuff(entity))
        {
            FrostDashProcThisTick.Add(entityToBuff, entity);
        }
    }

    public static bool IsChillBuff(Entity entity)
    {
        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return false;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);
        return prefabGUID.Equals(Frost_Vampire_Buff_Chill);
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

    public static bool TryGetBuffTarget(Entity entity, out Entity targetEntity)
    {
        targetEntity = Entity.Null;
        if (!EntityManager.HasComponent<Buff>(entity))
        {
            return false;
        }
        var buff = EntityManager.GetComponentData<Buff>(entity);
        targetEntity = buff.Target;
        return true;
    }

    public static void ModifyBadFrostDashes()
    {
        foreach (var (victim, ev) in FrostDashProcThisTick)
        {
            if (HitWhileNotChilledThisTick.Contains(victim))
            {
                RemoveFrostDashFreezeMods(ev);
            }
        }
    }

    public static void RemoveFrostDashFreezeMods(Entity entity)
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
