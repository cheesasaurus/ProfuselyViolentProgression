using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProjectM;

namespace ProfuselyViolentProgression.LoadoutLockdown;

[HarmonyPatch]
public static class Hooks
{
    public static event Action EarlyUpdateGroup_Updated;

    [EcsSystemUpdatePostfix(typeof(EarlyUpdateGroup), onlyWhenSystemRuns: false)]
    public static void EarlyUpdateGroup_Postfix()
    {
        EarlyUpdateGroup_Updated?.Invoke();
    }

}