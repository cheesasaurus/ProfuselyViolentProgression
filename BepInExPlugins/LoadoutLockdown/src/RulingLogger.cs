using System.Text;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.LoadoutLockdown.Rulings;
using Stunlock.Core;

namespace ProfuselyViolentProgression.LoadoutLockdown;

public static class RulingLogger
{
    public static bool Enabled = false;

    public static void LogItemEquip(RulingItemEquip ruling)
    {
        if (!Enabled) return;
        var sb = new StringBuilder();
        sb.AppendLine("Ruling Made.");
        sb.AppendLine($"  ValidateItemEquip");
        sb.AppendLine($"  IsAllowed: {ruling.IsAllowed}");
        sb.AppendLine($"  Judgement: {ruling.Judgement}");
        sb.Append($"  ShouldMoveToWastedWeaponSlotBeforeEquipping: {ruling.ShouldMoveToWastedWeaponSlotBeforeEquipping}");
        LogUtil.LogDebug(sb.ToString());
    }

    public static void LogItemMoveBetweenInventorySlots(RulingItemMoveBetweenInventorySlots ruling)
    {
        if (!Enabled) return;
        var sb = new StringBuilder();
        sb.AppendLine("Ruling Made.");
        sb.AppendLine($"  ValidateItemMoveBetweenInventorySlots");
        sb.AppendLine($"  IsAllowed: {ruling.IsAllowed}");
        sb.AppendLine($"  Judgement: {ruling.Judgement}");
        sb.Append($"  ShouldUnEquipItemBeforeMoving: {ruling.ShouldUnEquipItemBeforeMoving}");
        LogUtil.LogDebug(sb.ToString());
    }

    public static void LogTryAutoEquipAfterAddItem(RulingTryAutoEquipAfterAddItem ruling, PrefabGUID itemPrefabGUID)
    {
        if (!Enabled) return;
        var sb = new StringBuilder();
        sb.AppendLine("Ruling Made.");
        sb.AppendLine($"  ValidateTryAutoEquipAfterAddItem");
        sb.AppendLine($"  IsAllowed: {ruling.IsAllowed}");
        sb.AppendLine($"  Judgement: {ruling.Judgement}");
        sb.Append($"  Item: {DebugUtil.LookupPrefabName(itemPrefabGUID)}");
        LogUtil.LogDebug(sb.ToString());
    }

    public static void LogUnEquipItemFromDesignatedSlotToInventory(RulingUnEquipItemFromDesignatedSlotToInventory ruling)
    {
        if (!Enabled) return;
        var sb = new StringBuilder();
        sb.AppendLine("Ruling Made.");
        sb.AppendLine($"  ValidateUnEquipItemFromDesignatedSlotToInventory");
        sb.AppendLine($"  IsAllowed: {ruling.IsAllowed}");
        sb.Append($"  Judgement: {ruling.Judgement}");
        LogUtil.LogDebug(sb.ToString());
    }

    public static void LogItemDropFromInventory(RulingItemDropFromInventory ruling)
    {
        if (!Enabled) return;
        var sb = new StringBuilder();
        sb.AppendLine("Ruling Made.");
        sb.AppendLine($"  ValidateItemDropFromInventory");
        sb.AppendLine($"  IsAllowed: {ruling.IsAllowed}");
        sb.Append($"  Judgement: {ruling.Judgement}");
        LogUtil.LogDebug(sb.ToString());
    }

    

}
