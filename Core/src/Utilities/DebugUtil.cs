using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.Core.Utilities;

public static class DebugUtil
{

    public static void LogComponentTypes(Entity entity)
    {
        LogUtil.LogMessage("-------------------------------------------");
        var componentTypes = WorldUtil.Game.EntityManager.GetComponentTypes(entity);
        foreach (var componentType in componentTypes)
        {
            LogUtil.LogMessage(componentType.ToString());
        }
        LogUtil.LogMessage("-------------------------------------------");
    }

    public static void LogPrefabGuid(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<PrefabGUID>(entity))
        {
            return;
        }
        var prefabGuid = entityManager.GetComponentData<PrefabGUID>(entity);
        LogUtil.LogInfo($"  PrefabGUID: {LookupPrefabName(prefabGuid)}");
    }

    public static void LogBuffs(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<BuffBuffer>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  buffs:");
        var attackerBuffs = entityManager.GetBuffer<BuffBuffer>(entity);
        foreach (var buff in attackerBuffs)
        {
            LogUtil.LogInfo($"    {LookupPrefabName(buff.PrefabGuid)}");
        }
    }

    public static void LogSpellModSet(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasComponent<SpellModSetComponent>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  spell mods:");
        var smsc = entityManager.GetComponentData<SpellModSetComponent>(entity);
        var sm = smsc.SpellMods;
        LogUtil.LogInfo($"    count:{sm.Count}");
        LogUtil.LogInfo($"    0:{LookupPrefabName(sm.Mod0.Id)}");
        LogUtil.LogInfo($"    1:{LookupPrefabName(sm.Mod1.Id)}");
        LogUtil.LogInfo($"    2:{LookupPrefabName(sm.Mod2.Id)}");
        LogUtil.LogInfo($"    3:{LookupPrefabName(sm.Mod3.Id)}");
        LogUtil.LogInfo($"    4:{LookupPrefabName(sm.Mod4.Id)}");
        LogUtil.LogInfo($"    5:{LookupPrefabName(sm.Mod5.Id)}");
        LogUtil.LogInfo($"    6:{LookupPrefabName(sm.Mod6.Id)}");
        LogUtil.LogInfo($"    7:{LookupPrefabName(sm.Mod7.Id)}");
        LogUtil.LogInfo("     ----");
    }

    public static void LogApplyBuffOnGameplayEvent(Entity entity)
    {
        var entityManager = WorldUtil.Game.EntityManager;
        if (!entityManager.HasBuffer<ApplyBuffOnGameplayEvent>(entity))
        {
            return;
        }
        LogUtil.LogInfo($"  Buffs to apply on gameplay events:");
        var applyBuffs = entityManager.GetBuffer<ApplyBuffOnGameplayEvent>(entity);
        foreach (var buff in applyBuffs)
        {
            LogUtil.LogInfo($"    stacks:{buff.Stacks}");
            LogUtil.LogInfo($"    0:{LookupPrefabName(buff.Buff0)}");
            LogUtil.LogInfo($"    1:{LookupPrefabName(buff.Buff1)}");
            LogUtil.LogInfo($"    2:{LookupPrefabName(buff.Buff2)}");
            LogUtil.LogInfo($"    3:{LookupPrefabName(buff.Buff3)}");
            LogUtil.LogInfo($"    spellModSource:{buff.CustomAbilitySpellModsSource}");
            LogUtil.LogInfo("     ----");
        }
    }

    public static string LookupPrefabName(PrefabGUID prefabGuid)
    {
        var prefabCollectionSystem = WorldUtil.Game.GetExistingSystemManaged<PrefabCollectionSystem>();
        var prefabLookupMap = prefabCollectionSystem._PrefabLookupMap;
        if (prefabLookupMap.GuidToEntityMap.ContainsKey(prefabGuid))
        {
            return $"{prefabLookupMap.GetName(prefabGuid)} PrefabGuid({prefabGuid.GuidHash})";
        }
        return $"GUID Not Found {prefabGuid._Value}";
    }
    


}
