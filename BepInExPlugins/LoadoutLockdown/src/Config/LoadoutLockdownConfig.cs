using System.Collections.Generic;

namespace ProfuselyViolentProgression.LoadoutLockdown.Config;

public class LoadoutLockdownConfig
{
    public int WeaponSlots { get; set; } = 8;
    public bool ApplyPvpMenuSwapRulesToPVE { get; set; } = false;
    public RulesByType RulesByType { get; set; } = new();
    public List<string> ForbiddenByPrefab { get; set; } = new();
    public List<string> NotWaste { get; set; } = new();
    public List<string> AlwaysAllowSwapIntoSlot { get; set; } = new();
}

public enum FromMenuDuringPVP
{
    AllowSwapIntoWastedSlot,
    AllowSwapIntoFilledSlot,
}

public interface IForbiddable
{
    public bool Forbidden { get; set; }
}

public interface ISwappableFromMenuDuringPVP
{
    public FromMenuDuringPVP FromMenuDuringPVP { get; set; }
}

public class EquippableToHotbarRules : IForbiddable, ISwappableFromMenuDuringPVP
{
    public bool Forbidden { get; set; } = false;
    public FromMenuDuringPVP FromMenuDuringPVP { get; set; } = FromMenuDuringPVP.AllowSwapIntoWastedSlot;
    public bool RequiresHotbarSlot { get; set; } = true;
}

public class EquippableToOwnSlotRules : IForbiddable, ISwappableFromMenuDuringPVP
{
    public bool Forbidden { get; set; } = false;
    public FromMenuDuringPVP FromMenuDuringPVP { get; set; } = FromMenuDuringPVP.AllowSwapIntoFilledSlot; // different from hotbar rules, because this is how the vanilla game works
}

public class RulesByType
{
    public EquippableToOwnSlotRules Bag { get; set; } = new();
    public EquippableToOwnSlotRules Cloak { get; set; } = new();
    public EquippableToHotbarRules FishingPole { get; set; } = new();
    public EquippableToOwnSlotRules Headgear { get; set; } = new();
    public EquippableToOwnSlotRules MagicSource { get; set; } = new();
    public RulesByArmorType ArmorTypes { get; set; } = new();
    public RulesByWeaponType WeaponTypes { get; set; } = new();    
}

public class RulesByArmorType
{
    public EquippableToOwnSlotRules Chest { get; set; } = new();
    public EquippableToOwnSlotRules Legs { get; set; } = new();
    public EquippableToOwnSlotRules Gloves { get; set; } = new();
    public EquippableToOwnSlotRules Footgear { get; set; } = new();
}

public class RulesByWeaponType
{
    public EquippableToHotbarRules Axes { get; set; } = new();
    public EquippableToHotbarRules Claws { get; set; } = new();
    public EquippableToHotbarRules Crossbow { get; set; } = new();
    public EquippableToHotbarRules Daggers { get; set; } = new();
    public EquippableToHotbarRules Greatsword { get; set; } = new();
    public EquippableToHotbarRules Longbow { get; set; } = new();
    public EquippableToHotbarRules Mace { get; set; } = new();
    public EquippableToHotbarRules Pistols { get; set; } = new();
    public EquippableToHotbarRules Reaper { get; set; } = new();
    public EquippableToHotbarRules Slashers { get; set; } = new();
    public EquippableToHotbarRules Spear { get; set; } = new();
    public EquippableToHotbarRules Sword { get; set; } = new();
    public EquippableToHotbarRules Twinblades { get; set; } = new();
    public EquippableToHotbarRules Whip { get; set; } = new();
}
