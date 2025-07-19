using HarmonyLib;
using HookDOTS.API.Attributes;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class RenamingPatches
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;

    // this fires for renaming containers, but not renaming servants.
    [HarmonyPatch(typeof(NameableInteractableSystem), nameof(NameableInteractableSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void NameableInteractableSystem_OnUpdate_Prefix(NameableInteractableSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._RenameQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var renameInteractables = query.ToComponentDataArray<InteractEvents_Client.RenameInteractable>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            if (!networkIdToEntityMap.TryGetValue(renameInteractables[i].InteractableId, out var renameTarget))
            {
                continue;
            }

            if (!_entityManager.TryGetComponentData<CastleHeartConnection>(renameTarget, out var castleHeartConnection))
            {
                continue;
            }

            var character = fromCharacters[i].Character;

            var ruling = Core.RestrictionService.ValidateAction_RenameCastleStructure(
                actingCharacter: character,
                structureToRename: renameTarget,
                castleHeartConnection: castleHeartConnection
            );

            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }
        }
    }

    // todo: servants

}