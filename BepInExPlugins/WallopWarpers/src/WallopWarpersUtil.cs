using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Debugging;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProfuselyViolentProgression.WallopWarpers;

public static class WallopWarpersUtil
{

    private static EntityManager EntityManager => WorldUtil.Server.EntityManager;
    private static EndSimulationEntityCommandBufferSystem EndSimulationEntityCommandBufferSystem => WorldUtil.Server.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();

    public static bool IsUseWaypointCast(PrefabGUID prefabGUID)
    {
        return prefabGUID.Equals(PrefabGuids.AB_Interact_UseWaypoint_Cast)
            || prefabGUID.Equals(PrefabGuids.AB_Interact_UseWaypoint_Castle_Cast);
    }

    public static bool IsInPvpCombat(Entity character)
    {
        return BuffUtility.HasBuff(EntityManager, character, PrefabGuids.Buff_InCombat_PvPVampire);
    }

    public static void SendMessageTeleportDisallowed(Entity character)
    {
        CreateSCTMessage(character, SCTMessage_NoTeleport_InCombat_PvP, ColorRed);
    }

    public static AssetGuid SCTMessage_NoTeleport_InCombat_PvP = AssetGuid.FromString("d181286d-5bbc-481f-b5aa-3a104dda868f");
    public static float3 ColorRed = new float3(255, 0, 0);
    public static void CreateSCTMessage(Entity character, AssetGuid messageGuid, float3 color)
    {
        var translation = EntityManager.GetComponentData<Translation>(character);

        ScrollingCombatTextMessage.Create(
            EntityManager,
            EndSimulationEntityCommandBufferSystem.CreateCommandBuffer(),
            messageGuid,
            translation.Value,
            color,
            character
        //value,
        //sct,
        //player.UserEntity
        );
    }

    // the effect applied here is pretty fragile, it can get removed by all sorts of things. can't really rely on this
    public static void ImpairWaypointUse(Entity character)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(character);
        bfs.Value._Value |= (long)BuffModificationTypes.WaypointImpair;
        //bfs.Value._Value |= (long)BuffModificationTypes.LocalTeleporterImpaired; // already handled by the vanilla game
        EntityManager.SetComponentData(character, bfs);
    }

    public static void UnImpairWaypointUse(Entity character)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(character);
        bfs.Value._Value &= ~(long)BuffModificationTypes.WaypointImpair;
        EntityManager.SetComponentData(character, bfs);
    }

    public static void ModifyBuffBeforeSpawn_DoNotImpairWaypointUse(Entity entity)
    {
        if (EntityManager.TryGetComponentData<BuffModificationFlagData>(entity, out var bmfd))
        {
            bmfd.ModificationTypes &= ~(long)BuffModificationTypes.WaypointImpair;
            EntityManager.SetComponentData(entity, bmfd);
        }
    }

    // this doesn't work
    public static void InterruptCast1(Entity entity, AbilityCastStartedEvent ev)
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

    // this doesn't work either
    unsafe public static void InterruptCast2(Entity entity, AbilityCastStartedEvent ev)
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

    unsafe public static void InterruptCast3(Entity entity, AbilityCastStartedEvent ev)
    {
        // todo: try something with GameplayDebugRecorder.CreateAbilityEvent
    }
    
}
