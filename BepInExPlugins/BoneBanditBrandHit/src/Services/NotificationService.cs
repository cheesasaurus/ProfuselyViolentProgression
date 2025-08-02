using System;
using System.Collections.Generic;
using ProfuselyViolentProgression.BoneBanditBrandHit.Services;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.Network;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProfuselyViolentProgression.BoneBanditBrandHits.Services;

/// <summary>
/// Responsible for notfying players about denied actions.
/// </summary>
public class NotificationService
{
    SCTService _SCTService;
    Dictionary<Entity, DateTime> _lastChatNotifications = [];
    TimeSpan _chatNotificationCooldown = new TimeSpan(hours: 0, minutes: 0, seconds: 30);
    EntityManager _entityManager;

    public NotificationService(ref EntityManager entityManager, SCTService SCTService)
    {
        _entityManager = entityManager;
        _SCTService = SCTService;
    }
    public void NotifyDeathContainerAccessDenied(Entity character, Entity deathContainerEntity)
    {
        if (!_entityManager.TryGetComponentData<PlayerCharacter>(character, out var playerCharacter))
        {
            return;
        }
        if (!_entityManager.TryGetComponentData<User>(playerCharacter.UserEntity, out var user))
        {
            return;
        }

        SendSCTMessage(character, deathContainerEntity);
        SendChatNotification(character, user);
    }

    private void SendSCTMessage(Entity character, Entity deathContainerEntity)
    {
        var containerPos = _entityManager.GetComponentData<Translation>(deathContainerEntity).Value;
        var messagePos = containerPos - new float3(0, 1, 0);
        _SCTService.CreateSCTMessage(messagePos, character, SCTService.SCTMessage_MissingOwnership, SCTService.ColorDarkRed);
    }

    private void SendChatNotification(Entity character, User user)
    {
        if (IsChatNotificationOnCooldown(character))
        {
            return;
        }

        var message = "While PvP protected, you cannot loot death containers which don't belong to your clan.";
        ChatUtil.SendSystemMessageToClient(user, message);
        _lastChatNotifications[character] = DateTime.Now;
    }

    private bool IsChatNotificationOnCooldown(Entity character)
    {
        if (_lastChatNotifications.TryGetValue(character, out var lastChatNotificationDT))
        {
            var cooldownEndsDT = lastChatNotificationDT + _chatNotificationCooldown;
            if (DateTime.Now <= cooldownEndsDT)
            {
                return true;
            }
        }
        return false;
    }

}