using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class DoorPatches
{
    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;

    [HarmonyPatch(typeof(OpenDoorSystem), nameof(OpenDoorSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void OpenDoorSystem_OnUpdate_Prefix(OpenDoorSystem __instance)
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

            var ruling = Core.RestrictionService.ValidateAction_OpenOrCloseDoor(character, door);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                spellTarget.DestroyIfNotInteractable = true;
                spellTarget.Target = Entity.Null;
                EntityManager.SetComponentData(entities[i], spellTarget);
            }
        }
    }

}