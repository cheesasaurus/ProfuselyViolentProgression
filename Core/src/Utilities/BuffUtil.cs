using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.Core.Utilities;

public class BuffUtil
{
    public static void GiveBuffToPlayer(Entity character, PrefabGUID buffPrefabGUID)
    {
        var playerCharacter = WorldUtil.Server.EntityManager.GetComponentData<PlayerCharacter>(character);

        var fromCharacter = new FromCharacter()
        {
            User = playerCharacter.UserEntity,
            Character = character,
        };

        var buffEvent = new ApplyBuffDebugEvent()
        {
            BuffPrefabGUID = buffPrefabGUID
        };

        var debugEventsSystem = WorldUtil.Server.GetExistingSystemManaged<DebugEventsSystem>();
        debugEventsSystem.ApplyBuff(fromCharacter, buffEvent);
    }

    public static bool TryRemoveBuffFromPlayer(Entity character, PrefabGUID buffPrefabGUID)
    {
        var entityManager = WorldUtil.Server.EntityManager;
        var playerHasBuff = BuffUtility.TryGetBuff(entityManager, character, buffPrefabGUID, out var buffEntity);
        if (playerHasBuff)
        {
            DestroyUtility.Destroy(entityManager, buffEntity, DestroyDebugReason.TryRemoveBuff);
            return true;
        }
        return false;
    }
    
    public static void ToggleBuffableFlagState_On(Entity character, BuffModificationTypes flags)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(character);
        bfs.Value._Value |= (long)flags;
        EntityManager.SetComponentData(character, bfs);
        DebugUtil.LogBuffableFlagState(character);
    }

    public static void ToggleBuffableFlagState_Off(Entity character, BuffModificationTypes flag)
    {
        var EntityManager = WorldUtil.Server.EntityManager;
        var bfs = EntityManager.GetComponentData<BuffableFlagState>(character);
        bfs.Value._Value &= ~(long)flag;
        EntityManager.SetComponentData(character, bfs);
        DebugUtil.LogBuffableFlagState(character);
    }

}