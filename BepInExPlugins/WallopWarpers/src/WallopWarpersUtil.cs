using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
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

    public static void SendMessagePvPTeleportDisallowed(Entity character, Entity waygate)
    {
        var waygatePos = EntityManager.GetComponentData<Translation>(waygate).Value;
        var messagePos = waygatePos - new float3(0, 2, 0);
        CreateSCTMessage(messagePos, character, SCTMessage_NoTeleport_InCombat_PvP, ColorRed);
    }

    public static AssetGuid SCTMessage_NoTeleport_InCombat_PvP = AssetGuid.FromString("d181286d-5bbc-481f-b5aa-3a104dda868f");
    public static float3 ColorRed = new float3(255, 0, 0);
    private static void CreateSCTMessage(float3 worldPosition, Entity character, AssetGuid messageGuid, float3 color)
    {
        ScrollingCombatTextMessage.Create(
            EntityManager,
            EndSimulationEntityCommandBufferSystem.CreateCommandBuffer(),
            messageGuid,
            worldPosition,
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
        BuffUtil.ToggleBuffableFlagState_On(character, BuffModificationTypes.WaypointImpair);
    }

    public static void UnImpairWaypointUse(Entity character)
    {
        BuffUtil.ToggleBuffableFlagState_Off(character, BuffModificationTypes.WaypointImpair);
    }

    public static void ModifyBuffBeforeSpawn_DoNotImpairWaypointUse(Entity entity)
    {
        if (EntityManager.TryGetComponentData<BuffModificationFlagData>(entity, out var bmfd))
        {
            bmfd.ModificationTypes &= ~(long)BuffModificationTypes.WaypointImpair;
            EntityManager.SetComponentData(entity, bmfd);
        }
    }

    unsafe public static void InterruptCast(AbilityCastStartedEvent ev)
    {
        var ssm = WorldUtil.Server.GetExistingSystemManaged<ServerScriptMapper>();
        var sgm = ssm.GetServerGameManager();
        sgm.InterruptCast(ev.Character);
    }
    
}
