using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.BoneBanditBrandHit.Patches;


[HarmonyPatch]
public static class AbilityCastPatches
{

    private static EntityManager _entityManager = WorldUtil.Server.EntityManager;

    [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void AbilityRunScriptsSystem_Prefix(AbilityRunScriptsSystem __instance)
    {
        if (!Core.IsInitialized)
        {
            return;
        }
        var serverGameManager = Core.FetchServerGameManager();
        ProcessQuery_OnCastStarted(__instance, ref serverGameManager);
    }

    private static void ProcessQuery_OnCastStarted(AbilityRunScriptsSystem __instance, ref ServerGameManager serverGameManager)
    {
        var query = __instance._OnCastStartedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            OnCastStarted(ref serverGameManager, entities[i], events[i]);
        }
    }

    private static void OnCastStarted(ref ServerGameManager serverGameManager, Entity entity, AbilityCastStartedEvent ev)
    {
        if (!_entityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        if (!_entityManager.TryGetComponentData<AbilityTarget>(ev.Ability, out var abilityTarget))
        {
            return;
        }
        if (abilityTarget.GetTargetType is not AbilityTarget.Type.InteractTarget)
        {
            return;
        }

        if (_entityManager.HasComponent<PlayerDeathContainer>(abilityTarget.Target._Entity))
        {
            Handle_StartOpeningDeathContainer(ref serverGameManager, ev.Character, abilityTarget.Target._Entity);
        }
    }

    private static void Handle_StartOpeningDeathContainer(ref ServerGameManager serverGameManager, Entity character, Entity container)
    {
        if (!_entityManager.TryGetComponentData<PlayerDeathContainer>(container, out var playerDeathContainer))
        {
            return;
        }
        if (!_entityManager.TryGetComponentData<VampireSpecificAttributes>(character, out var vampireSpecificAttributes))
        {
            return;
        }
        if (!_entityManager.TryGetComponentData<Team>(character, out var characterTeam))
        {
            return;
        }
        if (!_entityManager.TryGetComponentData<Team>(playerDeathContainer.DeadUserEntity, out var containerTeam))
        {
            return;
        }

        bool isCharacterPvpProtected = vampireSpecificAttributes.PvPProtected._Value;
        bool isClanMemberContainer = characterTeam.Clan.Equals(containerTeam.Clan);

        if (isCharacterPvpProtected && !isClanMemberContainer)
        {
            Core.NotificationService.NotifyDeathContainerAccessDenied(character, container);
            serverGameManager.InterruptCast(character);
        }
    }

}