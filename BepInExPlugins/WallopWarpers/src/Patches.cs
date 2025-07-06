using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.WallopWarpers;

[HarmonyPatch]
public static class Patches
{
    public static PrefabGUID AB_Interact_UseWaypoint_Cast = new PrefabGUID(-402025199);
    public static PrefabGUID AB_Interact_UseWaypoint_Castle_Cast = new PrefabGUID(-1252882299);
    public static PrefabGUID Buff_InCombat_PvPVampire = new PrefabGUID(697095869);

    public static EntityManager EntityManager => WorldUtil.Server.EntityManager;

    [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void AbilityRunScriptsSystem_Prefix(AbilityRunScriptsSystem __instance)
    {
        ProcessQuery_OnCastStarted(__instance);
    }

    private static void ProcessQuery_OnCastStarted(AbilityRunScriptsSystem __instance)
    {
        var query = __instance._OnCastStartedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            var ev = events[i];
            OnCastStarted(entities[i], ev);
        }
    }

    private static void OnCastStarted(Entity entity, AbilityCastStartedEvent ev)
    {
        if (!EntityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        if (!EntityManager.TryGetComponentData<PrefabGUID>(ev.Ability, out var abilityPrefabGUID))
        {
            return;
        }

        var isUseWaypointCast = abilityPrefabGUID.Equals(AB_Interact_UseWaypoint_Cast)
            || abilityPrefabGUID.Equals(AB_Interact_UseWaypoint_Castle_Cast);

        if (isUseWaypointCast && BuffUtility.HasBuff(EntityManager, ev.Character, Buff_InCombat_PvPVampire))
        {
            //DebugUtil.LogComponentTypes(entity);
            //DebugUtil.LogInteractAbilityBuffer(ev.Character);
            ImpairWaypointUse(ev.Character);
            //InterruptCast1(entity, ev);
            InterruptCast2(entity, ev);
        }

        //var playerCharacter = EntityManager.GetComponentData<PlayerCharacter>(ev.Character);
        //var abilityPrefabName = DebugUtil.LookupPrefabName(ev.Ability);
        //var abilityGroupPrefabName = DebugUtil.LookupPrefabName(ev.AbilityGroup);
        //LogUtil.LogInfo($"CastStarted.\n  character: {playerCharacter.Name}\n  ability: {abilityPrefabName}\n  abilityGroup: {abilityGroupPrefabName}");
    }

    private static void ImpairWaypointUse(Entity entity)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        DebugUtil.LogBuffableFlagState(entity);
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(entity);
        bfs.Value._Value |= (long)BuffModificationTypes.WaypointImpair;
        //bfs.Value._Value |= (long)BuffModificationTypes.LocalTeleporterImpaired; // already handled by the vanilla game
        EntityManager.SetComponentData(entity, bfs);

        DebugUtil.LogBuffableFlagState(entity);
    }

    private static void InterruptCast1(Entity entity, AbilityCastStartedEvent ev)
    {
        var newEntity = EntityManager.CreateEntity(
            ComponentType.ReadWrite<AbilityInterruptedEvent>()
        );
        EntityManager.SetComponentData(newEntity, new AbilityInterruptedEvent
        {
            Character = ev.Character,
            AbilityGroup = ev.AbilityGroup,
            Ability = ev.Ability
        });
        // todo: I think we have to fiddle with some state in the ability group? or maybe the character?
        // or maybe it has to be interrupted after the AbilityCastStartedEvent processed by system (so postfix)?
        // use AbilityBarUtilty.Interrupt?
        // AbilityCastingState?
    }

    unsafe private static void InterruptCast2(Entity entity, AbilityCastStartedEvent ev)
    {
        if (!EntityManager.TryGetComponentData<AbilityBar_Shared>(ev.Character, out var abilityBar_Shared))
        {
            return;
        }
        var ecbs = WorldUtil.Server.GetExistingSystemManaged<EntityCommandBufferSystem>();
        var ssm = WorldUtil.Server.GetExistingSystemManaged<ServerScriptMapper>();
        var sgm = ssm.GetServerGameManager();

        var ascssHandle = WorldUtil.Server.GetExistingSystem<AbilityStartCastingSystem_Server>();
        var ascssRef = WorldUtil.Server.Unmanaged.GetUnsafeSystemRef<AbilityStartCastingSystem_Server>(ascssHandle);
        var eventPrefabs = ascssRef._EventPrefabs;

        var debugData = new DebugData
        {
            CurrentFrame = sgm.ServerFrame,
            IsClient = false,
        };

        var ecb = ecbs.CreateCommandBuffer();

        // todo: figure out what's going wrong here
        // maybe the cast hasn't successfully started yet and we are trying to interrupt it too early?
        LogUtil.LogDebug($"ev.Character null? {ev.Character.Equals(Entity.Null)}"); // not null
        AbilityBarUtility.Interrupt(ref ecb, ref abilityBar_Shared, ev.Character, sgm.ServerTime, ref eventPrefabs, ref debugData);
        // [Error  :Il2CppInterop] During invoking native->managed trampoline
        // Exception: Il2CppInterop.Runtime.Il2CppException: System.InvalidOperationException: Invalid Entity.Null passed.
        // --- BEGIN IL2CPP STACK TRACE ---
        // System.InvalidOperationException: Invalid Entity.Null passed.
        // at Unity.Entities.EntityCommandBuffer.CheckEntityNotNull (Unity.Entities.Entity entity) [0x00000] in <00000000000000000000000000000000>:0
        // at Unity.Entities.EntityCommandBuffer.Instantiate (Unity.Entities.Entity e) [0x00000] in <00000000000000000000000000000000>:0
        // at ProjectM.AbilityBarUtility.Interrupt (Unity.Entities.EntityCommandBuffer& commandBuffer, ProjectM.AbilityBar_Shared& abilityBar, Unity.Entities.Entity character, System.Double serverTime, ProjectM.EventPrefabs& eventPrefabs, ProjectM.DebugData& debugData) [0x00000] in <00000000000000000000000000000000>:0
        // --- END IL2CPP STACK TRACE ---
        // 
        // at Il2CppInterop.Runtime.Il2CppException.RaiseExceptionIfNecessary(IntPtr returnedException) in C:\Work\-\VRisingBepInExBuild\Il2CppInterop\Il2CppInterop.Runtime\Il2CppException.cs:line 36
        // at ProjectM.AbilityBarUtility.Interrupt(EntityCommandBuffer& commandBuffer, AbilityBar_Shared& abilityBar, Entity character, Double serverTime, EventPrefabs& eventPrefabs, DebugData& debugData)
        // at ProfuselyViolentProgression.WallopWarpers.Patches.InterruptCast2(Entity entity, AbilityCastStartedEvent ev)
    }

    // todo: maybe better to hook AbilityStartCastingSystem_Server?

    [EcsSystemUpdatePrefix(typeof(AbilityStartCastingSystem_Server))]
    unsafe public static void AbilityStartCastingSystem_Server_OnUpdate_Prefix(SystemState* systemState)
    {
        // todo
    }

}