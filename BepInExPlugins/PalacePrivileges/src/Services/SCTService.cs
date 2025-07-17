using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

// responsible for sending Scrolling Combat Text (SCT) messages
public class SCTService
{
    private EntityManager _entityManager = WorldUtil.Server.EntityManager;

    public static AssetGuid SCTMessage_Nope = AssetGuid.FromString("7114de17-65b2-4e69-8723-79f8b33b2213");
    public static AssetGuid SCTMessage_Disabled = AssetGuid.FromString("3bf7e066-4e49-4ae4-b7a3-6703b7a15dc1");
    public static AssetGuid SCTMessage_MissingOwnership = AssetGuid.FromString("3c901e55-974d-4aa2-862e-1f63da340dc7");

    public static float3 ColorRed = new float3(255, 0, 0);

    public void CreateSCTMessage(Entity character, AssetGuid messageGuid, float3 color)
    {
        var translation = _entityManager.GetComponentData<Translation>(character);
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        ScrollingCombatTextMessage.Create(
            _entityManager,
            ecb,
            messageGuid,
            translation.Value,
            color,
            character
        //value,
        //sct,
        //player.UserEntity
        );

        // todo: this is not ideal. the point of a buffer is to have it ran together with other structural changes
        ecb.Playback(_entityManager);
    }

    public void SendMessageNope(Entity character)
    {
        CreateSCTMessage(character, SCTMessage_Nope, ColorRed);
    }

    public void SendMessageMissingOwnership(Entity character)
    {
        CreateSCTMessage(character, SCTMessage_MissingOwnership, ColorRed);
    }
    
}
