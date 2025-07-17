using System;
using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

/// <summary>
/// Responsible for notfying players about denied actions.
/// </summary>
public class NotificationService
{
    SCTService _SCTService;
    Dictionary<ThrottledCombo, DateTime> _lastChatNotifications = [];
    TimeSpan _chatNotificationCooldown = new TimeSpan(hours: 0, minutes: 0, seconds: 30);

    public NotificationService(SCTService SCTService)
    {
        _SCTService = SCTService;
    }
    public void NotifyActionDenied(Entity character, ref CastleActionRuling ruling)
    {
        SendSCTMessage(character, ref ruling);
        SendChatNotification(character, ref ruling);
    }

    private void SendSCTMessage(Entity character, ref CastleActionRuling ruling)
    {
        // todo: other things depending on the situation
        _SCTService.SendMessageNope(character);
    }

    private void SendChatNotification(Entity character, ref CastleActionRuling ruling)
    {
        if (ruling.IsCastleWithoutOwner)
        {
            return;
        }

        var combo = new ThrottledCombo(ruling);
        if (IsChatNotificationOnCooldown(combo))
        {
            return;
        }

        var message = ChatMessageForActingUser(ref ruling);
        ChatUtil.SendSystemMessageToClient(ruling.ActingUser.User, message);
        _lastChatNotifications[combo] = DateTime.Now;
    }

    private bool IsChatNotificationOnCooldown(ThrottledCombo combo)
    {
        if (_lastChatNotifications.TryGetValue(combo, out var lastChatNotificationDT))
        {
            var cooldownEndsDT = lastChatNotificationDT + _chatNotificationCooldown;
            if (DateTime.Now <= cooldownEndsDT)
            {
                return true;
            }
        }
        return false;
    }

    private struct ThrottledCombo(CastleActionRuling ruling)
    {
        public ulong ActingUserPlatformId = ruling.ActingUser.PlatformId;
        public ulong CastleOwnerPlatformId = ruling.CastleOwner.PlatformId;
        public RestrictedCastleActions Action = ruling.Action;
    }

    private string ChatMessageForActingUser(ref CastleActionRuling ruling)
    {
        var ownerName = ruling.CastleOwner.CharacterName;

        switch (ruling.Action)
        {
            
            case RestrictedCastleActions.OpenDoor:
            case RestrictedCastleActions.CloseDoor:
                return $"{ownerName} has not given you permission to use that kind of door.";

            default:
                return $"{ownerName} has not given you permission to do that.";
        }
    }

}