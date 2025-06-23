using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.FrostDashFreezeFix;

public static class FixDashAttackTriggersUtil
{
    public static int TickCount = 0;
    public static int RecursiveGroupPassesThisTick = 0;
    public static string RecursiveGroupTickStamp { get => $"{TickCount}-{RecursiveGroupPassesThisTick}"; }

    private static Dictionary<Entity, InflictionFlagsState> InflictionsWhenFirstHitThisTick = new();
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;

    public static void NewTickStarted()
    {
        TickCount++;
        RecursiveGroupPassesThisTick = 0;
        InflictionsWhenFirstHitThisTick.Clear();
    }

    public static void RecursiveGroupUpdateStarting()
    {
        RecursiveGroupPassesThisTick++;
    }

    public static void EntityGotHitWithSomethingDamaging(Entity entity)
    {
        if (!InflictionsWhenFirstHitThisTick.ContainsKey(entity))
        {
            InflictionsWhenFirstHitThisTick.Add(entity, CheckInflictions(entity));
        }
    }

    public static void BuffWillBeSpawned(Entity entity, Entity entityToBuff)
    {
        if (!InflictionsWhenFirstHitThisTick.TryGetValue(entityToBuff, out var targetHad))
        {
            return;
        }

        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);

        if (!targetHad.Leech && prefabGUID.Equals(PrefabGUIDs.AB_Vampire_VeilOfBlood_TriggerBonusEffects))
        {
            RemoveSpellMods_RequiringTargetWithLeech(entity);
        }
        else if (!targetHad.Ignite && prefabGUID.Equals(PrefabGUIDs.AB_Vampire_VeilOfChaos_TriggerBonusEffects))
        {
            RemoveSpellMods_RequiringTargetWithIgnite(entity);
        }
        else if (!targetHad.Condemn && prefabGUID.Equals(PrefabGUIDs.AB_Vampire_VeilOfBones_TriggerBonusEffects))
        {
            RemoveSpellMods_RequiringTargetWithCondemn(entity);
        }
        else if (!targetHad.Weaken && prefabGUID.Equals(PrefabGUIDs.AB_Vampire_VeilOfIllusion_TriggerBonusEffects))
        {
            RemoveSpellMods_RequiringTargetWithWeaken(entity);
        }
        else if (!targetHad.Chill && prefabGUID.Equals(PrefabGUIDs.AB_Vampire_VeilOfFrost_TriggerBonusEffects))
        {
            RemoveSpellMods_RequiringTargetWithChill(entity);
        }
        else if (!targetHad.Static && prefabGUID.Equals(PrefabGUIDs.AB_Vampire_VeilOfStorm_TriggerBonusEffects))
        {
            RemoveSpellMods_RequiringTargetWithStatic(entity);
        }
    }

    public static InflictionFlagsState CheckInflictions(Entity entity)
    {
        InflictionFlagsState flags = new();
        if (!EntityManager.HasBuffer<BuffBuffer>(entity))
        {
            return flags;
        }

        var targetsBuffs = EntityManager.GetBuffer<BuffBuffer>(entity);
        foreach (var buff in targetsBuffs)
        {
            if (buff.PrefabGuid.Equals(PrefabGUIDs.Blood_Vampire_Buff_Leech))
            {
                flags.Set(InflictionFlags.Leech);
            }
            else if (buff.PrefabGuid.Equals(PrefabGUIDs.Chaos_Vampire_Buff_Ignite))
            {
                flags.Set(InflictionFlags.Ignite);
            }
            else if (buff.PrefabGuid.Equals(PrefabGUIDs.Unholy_Vampire_Buff_Condemn))
            {
                flags.Set(InflictionFlags.Condemn);
            }
            else if (buff.PrefabGuid.Equals(PrefabGUIDs.Illusion_Vampire_Buff_Weaken))
            {
                flags.Set(InflictionFlags.Weaken);
            }
            else if (buff.PrefabGuid.Equals(PrefabGUIDs.Frost_Vampire_Buff_Chill))
            {
                flags.Set(InflictionFlags.Chill);
            }
            else if (buff.PrefabGuid.Equals(PrefabGUIDs.Storm_Vampire_Buff_Static))
            {
                flags.Set(InflictionFlags.Static);
            }
        }
        return flags;
    }

    public static void RemoveSpellMods_RequiringTargetWithChill(Entity entity)
    {
        RemoveSpellMods(entity, PrefabGUIDs.SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack);
    }

    public static void RemoveSpellMods_RequiringTargetWithLeech(Entity entity)
    {
        RemoveSpellMods(entity, PrefabGUIDs.SpellMod_VeilOfBlood_Empower);
    }

    public static void RemoveSpellMods_RequiringTargetWithWeaken(Entity entity)
    {
        RemoveSpellMods(entity, PrefabGUIDs.SpellMod_Shared_Illusion_WeakenShield_OnAttack);
    }

    public static void RemoveSpellMods_RequiringTargetWithStatic(Entity entity)
    {
        RemoveSpellMods(entity, PrefabGUIDs.SpellMod_Shared_Storm_ConsumeStaticIntoStun);
    }

    public static void RemoveSpellMods_RequiringTargetWithIgnite(Entity entity)
    {
        RemoveSpellMods(entity, PrefabGUIDs.SpellMod_Shared_Chaos_ConsumeIgniteAgonizingFlames_OnAttack);
    }

    public static void RemoveSpellMods_RequiringTargetWithCondemn(Entity entity)
    {
        // I don't think any dash spell mods require condemn
    }

    public static void RemoveSpellMods(Entity entity, PrefabGUID prefabGUID)
    {
        if (!EntityManager.HasBuffer<SpellModSetComponent>(entity))
        {
            return;
        }
        var smsc = EntityManager.GetComponentData<SpellModSetComponent>(entity);
        var sm = smsc.SpellMods;

        if (sm.Mod0.Id.Equals(prefabGUID))
        {
            sm.Mod0.Id = PrefabGUIDs.NullPrefabGUID;
        }
        if (sm.Mod1.Id.Equals(prefabGUID))
        {
            sm.Mod1.Id = PrefabGUIDs.NullPrefabGUID;
        }
        if (sm.Mod2.Id.Equals(prefabGUID))
        {
            sm.Mod2.Id = PrefabGUIDs.NullPrefabGUID;
        }
        if (sm.Mod3.Id.Equals(prefabGUID))
        {
            sm.Mod3.Id = PrefabGUIDs.NullPrefabGUID;
        }
        if (sm.Mod4.Id.Equals(prefabGUID))
        {
            sm.Mod4.Id = PrefabGUIDs.NullPrefabGUID;
        }
        if (sm.Mod5.Id.Equals(prefabGUID))
        {
            sm.Mod5.Id = PrefabGUIDs.NullPrefabGUID;
        }
        if (sm.Mod6.Id.Equals(prefabGUID))
        {
            sm.Mod6.Id = PrefabGUIDs.NullPrefabGUID;
        }
        if (sm.Mod7.Id.Equals(prefabGUID))
        {
            sm.Mod7.Id = PrefabGUIDs.NullPrefabGUID;
        }
        smsc.SpellMods = sm;
        EntityManager.SetComponentData(entity, smsc);
    }

}
