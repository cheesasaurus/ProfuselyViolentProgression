using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class DebugPatches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;

    [HarmonyPatch(typeof(NameableInteractableSystem), nameof(NameableInteractableSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void SomePatchThing(NameableInteractableSystem __instance)
    {
        // todo: renaming servants
    }

    //[HarmonyPatch(typeof(DoorSystem_Server), nameof(DoorSystem_Server.OnUpdate))]
    //[HarmonyPrefix]
    public static void SomePatchThing2()
    {
        // todo
    }

    [EcsSystemUpdatePrefix(typeof(OpenDoorsSystem))]
    public static void SomePatchThing3()
    {
        // todo
    }

}