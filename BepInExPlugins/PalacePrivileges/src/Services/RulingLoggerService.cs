using System.Text;
using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

public class RulingLoggerService
{
    public static bool Enabled = false;

    private ManualLogSource _log;

    public RulingLoggerService(ManualLogSource log)
    {
        _log = log;
    }

    public void LogRuling(CastleActionRuling ruling)
    {
        if (!Enabled) return;

        var sb = new StringBuilder();
        sb.AppendLine("Ruling Made.");

        if (ruling.NotEnoughDataToDecide)
        {
            sb.AppendLine($"  NOT ENOUGH DATA TO MAKE PROPER RULING");
            if (ruling.NotEnoughDataReason is not null)
            {
                sb.AppendLine($"  REASON: {ruling.NotEnoughDataReason}");
            }
        }

        sb.AppendLine($"  Target: {DebugUtil.LookupPrefabName(ruling.TargetPrefabGUID)}");
        sb.AppendLine($"  Action: {ruling.Action}");
        sb.AppendLine($"  IsAllowed: {ruling.IsAllowed}");
        sb.AppendLine($"  ActingUser: {ruling.ActingUser.CharacterName}");
        sb.AppendLine($"  CastleOwner: {ruling.CastleOwner.CharacterName}");
        sb.AppendLine($"  IsOwnerOfCastle: {ruling.IsOwnerOfCastle}");
        sb.AppendLine($"  IsCastleWithoutOwner: {ruling.IsCastleWithoutOwner}");
        sb.AppendLine($"  IsDefenseDisabled: {ruling.IsDefenseDisabled}");
        sb.AppendLine($"  IsSameClan: {ruling.IsSameClan}");
        sb.AppendLine($"  PermissiblePrivs: {ruling.PermissiblePrivs}");
        sb.AppendLine($"  ActingUserPrivs: {ruling.ActingUserPrivs}");
        _log.LogDebug(sb.ToString());
    }
    
}