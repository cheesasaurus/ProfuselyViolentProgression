using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class ServantGearPatches
{
    private const bool SKIP_ORIGINAL_METHOD = false;
    private const bool EXECUTE_ORIGINAL_METHOD = true;

    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;


    [HarmonyPatch(typeof(EquipServantItemFromInventorySystem), nameof(EquipServantItemFromInventorySystem.OnUpdate))]
    [HarmonyPrefix]
    public static void EquipServantItemFromInventorySystem_OnUpdate_Prefix(EquipServantItemFromInventorySystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._Query;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var equipEvents = query.ToComponentDataArray<EquipServantItemFromInventoryEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = equipEvents[i];

            if (!networkIdToEntityMap.TryGetValue(ev.ToEntity, out var servant))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_ServantGearChange(character, servant);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }

        }
    }

    [HarmonyPatch(typeof(UnEquipServantItemSystem), nameof(UnEquipServantItemSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void UnEquipServantItemSystem_OnUpdate_Prefix(UnEquipServantItemSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._Query;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var equipEvents = query.ToComponentDataArray<UnequipServantItemEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = equipEvents[i];

            if (!networkIdToEntityMap.TryGetValue(ev.FromEntity, out var servant))
            {
                continue;
            }

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_ServantGearChange(character, servant);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }

        }
    }

    /// <summary>
    /// EquipmentTransferSystem handles EquipmentToEquipmentTransferEvent.
    /// For example, swapping equipped chest pieces between a player and a servant.
    /// (even if one of the equip slots is empty)
    /// </summary>
    [HarmonyPatch(typeof(EquipmentTransferSystem), nameof(EquipmentTransferSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void EquipmentTransferSystem_OnUpdate_Prefix(EquipmentTransferSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._Query;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var equipEvents = query.ToComponentDataArray<EquipmentToEquipmentTransferEvent>(Allocator.Temp);

        var networkIdToEntityMap = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            var ev = equipEvents[i];

            // dragging from player slot to servant slot: toEntity is the servant.
            // dragging from servant slot to player slot: toEntity is the servant.
            // (yes, in both cases it is the servant.)
            if (!networkIdToEntityMap.TryGetValue(ev.ToEntity, out var servant))
            {
                continue;
            }            

            var character = fromCharacters[i].Character;
            var ruling = Core.RestrictionService.ValidateAction_ServantGearChange(character, servant);
            if (!ruling.IsAllowed)
            {
                Core.NotificationService.NotifyActionDenied(character, ref ruling);
                _entityManager.DestroyEntity(entities[i]);
            }
        }

    }

}