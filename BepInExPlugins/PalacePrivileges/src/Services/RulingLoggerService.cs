using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.PalacePrivileges.Models;
using ProjectM;
using Unity.Entities;

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
        LogUtil.LogDebug(sb.ToString());
    }
}