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
    
    private void SendSCTMessage(Entity character, ref CastleActionRuling ruling)
    {
        switch (ruling.Action)
        {
            case RestrictedCastleActions.BuildUseTreasury:
                _SCTService.SendMessageMissingResources(character);
                break;

            default:
                _SCTService.SendMessageNope(character);
                return;
        }        
    }

    private string ChatMessageForActingUser(ref CastleActionRuling ruling)
    {
        var ownerName = ruling.CastleOwner.CharacterName;

        switch (ruling.Action)
        {
            case RestrictedCastleActions.CastleHeartAbandon:
                return $"Only the owner ({ownerName}) may abandon their castle.";

            case RestrictedCastleActions.CastleHeartExpose:
                return $"Only the owner ({ownerName}) may expose their castle.";

            case RestrictedCastleActions.CastleHeartRemoveFuel:
                return $"Only the owner ({ownerName}) may remove essence from their castle heart.";

            case RestrictedCastleActions.CastleHeartRelocate:
                return $"Only the owner ({ownerName}) may relocate their castle.";

            case RestrictedCastleActions.CastleHeartDisableDefense:
                var hoursRemaining = 48; // todo: actual
                return $"Cannot key the castle of recent clanmates. {hoursRemaining} hours remaining.";

            case RestrictedCastleActions.Build:
                return $"{ownerName} has not given you permission to build.";

            case RestrictedCastleActions.BuildUseTreasury:
                return $"{ownerName} has not given you permission to use the treasury for building.";

            case RestrictedCastleActions.SowSeed:
                return $"{ownerName} has not given you permission to sow that seed.";

            case RestrictedCastleActions.PlantTree:
                return $"{ownerName} has not given you permission to plant that tree.";

            case RestrictedCastleActions.CraftItem:
                return $"{ownerName} has not given you permission to craft.";

             case RestrictedCastleActions.CraftUseTreasury:
                return $"{ownerName} has not given you permission to use the treasury for crafting.";

            case RestrictedCastleActions.OpenDoor:
            case RestrictedCastleActions.CloseDoor:
                return $"{ownerName} has not given you permission to use that kind of door.";

            case RestrictedCastleActions.RenameStructure:
                return $"{ownerName} has not given you permission to rename castle structures.";

            case RestrictedCastleActions.ServantConvert:
                return $"{ownerName} has not given you permission to convert servants.";

            case RestrictedCastleActions.ServantTerminate:
                return $"{ownerName} has not given you permission to terminate servants.";

            case RestrictedCastleActions.ServantRename:
                return $"{ownerName} has not given you permission to rename servants.";

            case RestrictedCastleActions.ServantGearChange:
                return $"{ownerName} has not given you permission to change servant gear.";

            case RestrictedCastleActions.PrisonerSubdue:
                return $"{ownerName} has not given you permission to subdue prisoners.";

            case RestrictedCastleActions.PrisonerKill:
                return $"{ownerName} has not given you permission to kill prisoners.";

            case RestrictedCastleActions.PrisonerExtractBlood:
                return $"{ownerName} has not given you permission to extract blood from prisoners.";

            case RestrictedCastleActions.PrisonerFeedSafeFood:
                return $"{ownerName} has not given you permission to feed safe food to prisoners.";

            case RestrictedCastleActions.PrisonerFeedUnSafeFood:
                return $"{ownerName} has not given you permission to feed unsafe food to prisoners.";

            case RestrictedCastleActions.WaygateIn:
                return $"{ownerName} has not given you permission to waygate in to their castle.";

            case RestrictedCastleActions.WaygateOut:
                return $"{ownerName} has not given you permission to waygate out of their castle.";

            case RestrictedCastleActions.UseTeleporterRed:
                return $"{ownerName} has not given you permission to use their red teleporters.";

            case RestrictedCastleActions.UseTeleporterYellow:
                return $"{ownerName} has not given you permission to use their yellow teleporters.";

            case RestrictedCastleActions.UseTeleporterPurple:
                return $"{ownerName} has not given you permission to use their purple teleporters.";

            case RestrictedCastleActions.UseTeleporterBlue:
                return $"{ownerName} has not given you permission to use their blue teleporters.";

            case RestrictedCastleActions.AccessLockbox:
                return $"{ownerName} has not given you permission to use their lockbox.";

            case RestrictedCastleActions.AccessMusicbox:
                return $"{ownerName} has not given you permission to use their music box.";

            case RestrictedCastleActions.AccessArenaStation:
                return $"{ownerName} has not given you permission to use their arena station.";

            case RestrictedCastleActions.AccessThrone:
                return $"{ownerName} has not given you permission to use their throne.";

            case RestrictedCastleActions.AccessRedistributionEngine:
                return $"{ownerName} has not given you permission to alter redistribution.";

            case RestrictedCastleActions.AccessResearchDeskT1:
                return $"{ownerName} has not given you permission to use their research desk.";

            case RestrictedCastleActions.AccessResearchDeskT2:
                return $"{ownerName} has not given you permission to use their study.";

            case RestrictedCastleActions.AccessResearchDeskT3:
                return $"{ownerName} has not given you permission to use their athenaeum.";

            case RestrictedCastleActions.AccessStygianAltar:
                return $"{ownerName} has not given you permission to use their stygian altar.";

            default:
                return $"{ownerName} has not given you permission to do that.";
        }
    }

}