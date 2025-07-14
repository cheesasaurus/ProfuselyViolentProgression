using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProjectM;

namespace ProfuselyViolentProgression.PalacePrivileges;

[HarmonyPatch]
public static class Hooks
{
    public static event Action EarlyUpdateGroup_Updated;
    public static event Action BeforeWorldSave;

    [EcsSystemUpdatePostfix(typeof(EarlyUpdateGroup), onlyWhenSystemRuns: false)]
    public static void EarlyUpdateGroup_Postfix()
    {
        EarlyUpdateGroup_Updated?.Invoke();
    }

    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    [HarmonyPrefix]
    public static void TriggerSave_Prefix()
    {
        BeforeWorldSave?.Invoke();
    }

}