using Unity.Entities;

namespace ProfuselyViolentProgression.LoadoutLockdown.Rulings;


public enum Judgement
{
    Allowed_Exception,
    Allowed_NoPlayerCharactersInvolved,
    Allowed_NotInRestrictiveCombat,
    Allowed_NoEquipmentSlotsInvolved,
    Allowed_NoEquipmentSlotRequired,
    Allowed_RearrangeWeaponSlots,
    Allowed_EquipmentInValidSlot,
    Allowed_InsertIntoEmptySlot,
    Allowed_SwapIntoWastedSlot,
    Allowed_EquipmentCanAlwaysBeSwappedIntoAppropriateSlot,
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


