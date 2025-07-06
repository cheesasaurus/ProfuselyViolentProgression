using ProjectM;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.LoadoutLockdown.Rulings;


public enum Judgement
{
    Allowed_Exception,
    Allowed_NoPlayerCharactersInvolved,
    Allowed_Cosmetic,
    Allowed_NotInRestrictiveCombat,
    Allowed_NoEquipmentSlotsInvolved,
    Allowed_NoEquipmentSlotRequired,
    Allowed_RearrangeWeaponSlots,
    Allowed_EquipmentInValidSlot,
    Allowed_InsertIntoEmptySlot,
    Allowed_DropFromWastedSlot,
    Allowed_SwapIntoWastedSlot,
    Allowed_EquipmentCanAlwaysBeSwappedIntoAppropriateSlot,
    Allowed_TryAutoEquipAlwaysAllowedUnlessEquipmentToEquipIsForbidden,
    Allowed_EquipmentCanAlwaysBeUnEquipped,
    Disallowed_EquipmentToEquipIsForbidden,
    Disallowed_CannotMenuSwapDuringPvPCombat,
    Disallowed_CannotMenuSwapDuringAnyCombat,
    Disallowed_NoFreeWeaponSlots,
}

public enum CombatRestriction
{
    None,
    AnyCombat,
    PvPCombat,
}

public enum EquipmentSlotStatus
{
    Unknown,
    Empty,
    FilledWasted,
    FilledNotWasted,
}

public struct RulingItemMoveBetweenInventorySlots
{
    public Judgement Judgement;
    public bool IsAllowed;
    public bool ShouldUnEquipItemBeforeMoving;
    public EquippedItem ItemToUnEquip;

    public struct EquippedItem
    {
        public Entity Character;
        public Entity Item;
    }

    public static RulingItemMoveBetweenInventorySlots Allowed(Judgement judgement) => new RulingItemMoveBetweenInventorySlots
    {
        Judgement = judgement,
        IsAllowed = true,
        ShouldUnEquipItemBeforeMoving = false
    };

    public static RulingItemMoveBetweenInventorySlots Disallowed(Judgement judgement) => new RulingItemMoveBetweenInventorySlots
    {
        Judgement = judgement,
        IsAllowed = false,
        ShouldUnEquipItemBeforeMoving = false
    };
}

public struct RulingItemEquip
{
    public Judgement Judgement;
    public bool IsAllowed;
    public bool ShouldMoveToWastedWeaponSlotBeforeEquipping;
    public int WastedWeaponSlotIndex;
    public EquippableItem ItemToEquip;

    public struct EquippableItem
    {
        public Entity Entity;
        public PrefabGUID PrefabGUID;
        public EquippableData EquippableData;
    }

    public static RulingItemEquip Allowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = true,
        ShouldMoveToWastedWeaponSlotBeforeEquipping = false
    };

    public static RulingItemEquip AllowedAfterMoveToWastedWeaponSlot(Judgement judgement,int slotIndex) => new()
    {
        Judgement = judgement,
        IsAllowed = true,
        ShouldMoveToWastedWeaponSlotBeforeEquipping = true,
        WastedWeaponSlotIndex = slotIndex
    };

    public static RulingItemEquip Disallowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = false,
        ShouldMoveToWastedWeaponSlotBeforeEquipping = false
    };
}


public struct RulingTryAutoEquipAfterAddItem
{
    public Judgement Judgement;
    public bool IsAllowed;

    public static RulingTryAutoEquipAfterAddItem Allowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = true,
    };

    public static RulingTryAutoEquipAfterAddItem Disallowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = false,
    };

}


public struct RulingUnEquipItemFromDesignatedSlotToInventory
{
    public Judgement Judgement;
    public bool IsAllowed;

    public static RulingUnEquipItemFromDesignatedSlotToInventory Allowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = true,
    };

    public static RulingUnEquipItemFromDesignatedSlotToInventory Disallowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = false,
    };
}

public struct RulingItemDropFromInventory
{
    public Judgement Judgement;
    public bool IsAllowed;

    public static RulingItemDropFromInventory Allowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = true,
    };

    public static RulingItemDropFromInventory Disallowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = false,
    };
}

public struct RulingItemDropFromDesignatedSlot
{
    public Judgement Judgement;
    public bool IsAllowed;

    public static RulingItemDropFromDesignatedSlot Allowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = true,
    };

    public static RulingItemDropFromDesignatedSlot Disallowed(Judgement judgement) => new()
    {
        Judgement = judgement,
        IsAllowed = false,
    };
}


