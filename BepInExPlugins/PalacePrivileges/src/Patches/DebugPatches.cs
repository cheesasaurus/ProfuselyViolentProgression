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

    [HarmonyPatch(typeof(OpenDoorSystem), nameof(OpenDoorSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void SomePatchThing(OpenDoorSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance.__query_1834203323_0;
        var entities = query.ToEntityArray(Allocator.Temp);
        var entityOwners = query.ToComponentDataArray<EntityOwner>(Allocator.Temp); // acting players
        var spellTargets = query.ToComponentDataArray<SpellTarget>(Allocator.Temp); // targeting a door

        for (var i = 0; i < entities.Length; i++)
        {
            var spellTarget = spellTargets[i];
            var character = entityOwners[i].Owner;
            var door = spellTarget.Target._Entity;

            if (Core.RestrictionService.ShouldDisallowAction_OpenDoor(character, door))
            {
                Core.SCTService.SendMessageNope(character);

                spellTarget.DestroyIfNotInteractable = true;
                spellTarget.Target = Entity.Null;
                EntityManager.SetComponentData(entities[i], spellTarget);
            }
        }
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