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
public unsafe class DebugPatches
{
    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;

    
    //[HarmonyPatch(typeof(NameableInteractableSystem), nameof(NameableInteractableSystem.OnUpdate))]
    //[HarmonyPrefix]
    public static void SomePatchThing(NameableInteractableSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }

        var query = __instance._RenameQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var fromCharacters = query.ToComponentDataArray<FromCharacter>(Allocator.Temp);
        var renameInteractables = query.ToComponentDataArray<InteractEvents_Client.RenameInteractable>(Allocator.Temp);

        //var networkIdLookup = Core.NetworkIdService.NetworkIdToEntityMap();

        for (var i = 0; i < entities.Length; i++)
        {
            
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