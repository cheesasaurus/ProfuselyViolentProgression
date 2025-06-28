using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.LoadoutLockdown.Config;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;

namespace ProfuselyViolentProgression.LoadoutLockdown;


internal class LoadoutLockdownService
{
    public static LoadoutLockdownService Instance;

    private LoadoutLockdownConfig _config;
    private EntityManager EntityManager = WorldUtil.Game.EntityManager;

    private int _maxSlotIndex => _config.WeaponSlots - 1;

    public LoadoutLockdownService(LoadoutLockdownConfig config)
    {
        _config = config;
        // todo: prefabs
    }

    public bool IsEquipmentForbidden(Entity entity)
    {
        if (!EntityManager.HasComponent<EquippableData>(entity))
        {
            return false;
        }
        var equippableData = EntityManager.GetComponentData<EquippableData>(entity);
        if (TryFindForbiddable(equippableData, out var forbiddable))
        {
            if (forbiddable.Forbidden)
            {
                return true;
            }
        }

        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return false;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);
        // todo: check prefab guid


        return false;
    }

    public bool IsEquippableWithoutSlot(Entity entity)
    {
        if (!EntityManager.HasComponent<EquippableData>(entity))
        {
            return false;
        }
        var equippableData = EntityManager.GetComponentData<EquippableData>(entity);
        if (TryFindEquippableToHotBarRules(equippableData, out var rules))
        {
            return !rules.RequiresHotbarSlot;
        }
        return false;
    }

    private static HashSet<EquipmentType> EquipmentTypesWithOwnSlot = [
        EquipmentType.Bag,
        EquipmentType.Cloak,
        EquipmentType.Headgear,
        EquipmentType.MagicSource,
        EquipmentType.Chest,
        EquipmentType.Legs,
        EquipmentType.Gloves,
        EquipmentType.Footgear,
    ];

    public bool HasOwnSlot(Entity entity)
    {
        if (!EntityManager.HasComponent<EquippableData>(entity))
        {
            return false;
        }
        var equippableData = EntityManager.GetComponentData<EquippableData>(entity);
        return EquipmentTypesWithOwnSlot.Contains(equippableData.EquipmentType);
    }

    public bool IsOwnSlotWasted(Entity character, Entity equippableEntity)
    {
        if (!HasOwnSlot(equippableEntity))
        {
            return false;
        }

        if (!EntityManager.HasComponent<EquippableData>(equippableEntity))
        {
            return false;
        }
        var equippableData = EntityManager.GetComponentData<EquippableData>(equippableEntity);

        if (!EntityManager.HasComponent<Equipment>(character))
        {
            return false;
        }
        var equipment = EntityManager.GetComponentData<Equipment>(character);
        if (!equipment.IsEquipped(equippableData.EquipmentType))
        {
            return true;
        }

        // todo: if it's a soulshard, should be able to put it on
        // but make that configurable

        // could potentially compare the gear (e.g. levels), but let's not overcomplicate it for something that nobody is asking for.
        //var entityInSlot = equipment.GetEquipmentEntity(equippableData.EquipmentType)._Entity;
        return false;
    }

    public bool CanMenuSwapIntoFilledSlotDuringPVP(EquippableData equippableData)
    {
        if (TryFindSwappableFromMenuDuringPVP(equippableData, out var swappableRules))
        {
            return swappableRules.FromMenuDuringPVP == FromMenuDuringPVP.AllowSwapIntoFilledSlot;
        }
        return false;
    }

    private bool TryFindForbiddable(EquippableData equippableData, out IForbiddable forbiddable)
    {
        switch (equippableData.EquipmentType)
        {
            case EquipmentType.Bag:
                forbiddable = _config.RulesByType.Bag;
                return true;
            case EquipmentType.Cloak:
                forbiddable = _config.RulesByType.Cloak;
                return true;
            case EquipmentType.Headgear:
                forbiddable = _config.RulesByType.Headgear;
                return true;
            case EquipmentType.MagicSource:
                forbiddable = _config.RulesByType.MagicSource;
                return true;

            case EquipmentType.Chest:
                forbiddable = _config.RulesByType.ArmorTypes.Chest;
                return true;
            case EquipmentType.Legs:
                forbiddable = _config.RulesByType.ArmorTypes.Chest;
                return true;
            case EquipmentType.Gloves:
                forbiddable = _config.RulesByType.ArmorTypes.Chest;
                return true;
            case EquipmentType.Footgear:
                forbiddable = _config.RulesByType.ArmorTypes.Chest;
                return true;

            case EquipmentType.Weapon:
                switch (equippableData.WeaponType)
                {
                    case WeaponType.FishingPole:
                        forbiddable = _config.RulesByType.FishingPole;
                        return true;
                    case WeaponType.Axes:
                        forbiddable = _config.RulesByType.WeaponTypes.Axes;
                        return true;
                    case WeaponType.Claws:
                        forbiddable = _config.RulesByType.WeaponTypes.Claws;
                        return true;
                    case WeaponType.Crossbow:
                        forbiddable = _config.RulesByType.WeaponTypes.Crossbow;
                        return true;
                    case WeaponType.Daggers:
                        forbiddable = _config.RulesByType.WeaponTypes.Daggers;
                        return true;
                    case WeaponType.GreatSword:
                        forbiddable = _config.RulesByType.WeaponTypes.Greatsword;
                        return true;
                    case WeaponType.Longbow:
                        forbiddable = _config.RulesByType.WeaponTypes.Longbow;
                        return true;
                    case WeaponType.Mace:
                        forbiddable = _config.RulesByType.WeaponTypes.Mace;
                        return true;
                    case WeaponType.Pistols:
                        forbiddable = _config.RulesByType.WeaponTypes.Pistols;
                        return true;
                    case WeaponType.Scythe:
                        forbiddable = _config.RulesByType.WeaponTypes.Reaper;
                        return true;
                    case WeaponType.Slashers:
                        forbiddable = _config.RulesByType.WeaponTypes.Slashers;
                        return true;
                    case WeaponType.Spear:
                        forbiddable = _config.RulesByType.WeaponTypes.Spear;
                        return true;
                    case WeaponType.Sword:
                        forbiddable = _config.RulesByType.WeaponTypes.Sword;
                        return true;
                    case WeaponType.Twinblades:
                        forbiddable = _config.RulesByType.WeaponTypes.Twinblades;
                        return true;
                    case WeaponType.Whip:
                        forbiddable = _config.RulesByType.WeaponTypes.Whip;
                        return true;
                    default:
                        forbiddable = default;
                        return false;
                }

            default:
                forbiddable = default;
                return false;
        }
    }

    // why oh why can i not have a generic where "OR" for segregated interfaces?
    // the logic here is exactly the same as TryFindForbiddable :(
    private bool TryFindSwappableFromMenuDuringPVP(EquippableData equippableData, out ISwappableFromMenuDuringPVP swappableRules)
    {
        switch (equippableData.EquipmentType)
        {
            case EquipmentType.Bag:
                swappableRules = _config.RulesByType.Bag;
                return true;
            case EquipmentType.Cloak:
                swappableRules = _config.RulesByType.Cloak;
                return true;
            case EquipmentType.Headgear:
                swappableRules = _config.RulesByType.Headgear;
                return true;
            case EquipmentType.MagicSource:
                swappableRules = _config.RulesByType.MagicSource;
                return true;

            case EquipmentType.Chest:
                swappableRules = _config.RulesByType.ArmorTypes.Chest;
                return true;
            case EquipmentType.Legs:
                swappableRules = _config.RulesByType.ArmorTypes.Chest;
                return true;
            case EquipmentType.Gloves:
                swappableRules = _config.RulesByType.ArmorTypes.Chest;
                return true;
            case EquipmentType.Footgear:
                swappableRules = _config.RulesByType.ArmorTypes.Chest;
                return true;

            case EquipmentType.Weapon:
                switch (equippableData.WeaponType)
                {
                    case WeaponType.FishingPole:
                        swappableRules = _config.RulesByType.FishingPole;
                        return true;
                    case WeaponType.Axes:
                        swappableRules = _config.RulesByType.WeaponTypes.Axes;
                        return true;
                    case WeaponType.Claws:
                        swappableRules = _config.RulesByType.WeaponTypes.Claws;
                        return true;
                    case WeaponType.Crossbow:
                        swappableRules = _config.RulesByType.WeaponTypes.Crossbow;
                        return true;
                    case WeaponType.Daggers:
                        swappableRules = _config.RulesByType.WeaponTypes.Daggers;
                        return true;
                    case WeaponType.GreatSword:
                        swappableRules = _config.RulesByType.WeaponTypes.Greatsword;
                        return true;
                    case WeaponType.Longbow:
                        swappableRules = _config.RulesByType.WeaponTypes.Longbow;
                        return true;
                    case WeaponType.Mace:
                        swappableRules = _config.RulesByType.WeaponTypes.Mace;
                        return true;
                    case WeaponType.Pistols:
                        swappableRules = _config.RulesByType.WeaponTypes.Pistols;
                        return true;
                    case WeaponType.Scythe:
                        swappableRules = _config.RulesByType.WeaponTypes.Reaper;
                        return true;
                    case WeaponType.Slashers:
                        swappableRules = _config.RulesByType.WeaponTypes.Slashers;
                        return true;
                    case WeaponType.Spear:
                        swappableRules = _config.RulesByType.WeaponTypes.Spear;
                        return true;
                    case WeaponType.Sword:
                        swappableRules = _config.RulesByType.WeaponTypes.Sword;
                        return true;
                    case WeaponType.Twinblades:
                        swappableRules = _config.RulesByType.WeaponTypes.Twinblades;
                        return true;
                    case WeaponType.Whip:
                        swappableRules = _config.RulesByType.WeaponTypes.Whip;
                        return true;
                    default:
                        swappableRules = default;
                        return false;
                }

            default:
                swappableRules = default;
                return false;
        }
    }

    private bool TryFindEquippableToHotBarRules(EquippableData equippableData, out EquippableToHotbarRules rules)
    {
        if (equippableData.EquipmentType is not EquipmentType.Weapon)
        {
            rules = default;
            return false;
        }
        switch (equippableData.WeaponType)
        {
            case WeaponType.FishingPole:
                rules = _config.RulesByType.FishingPole;
                return true;
            case WeaponType.Axes:
                rules = _config.RulesByType.WeaponTypes.Axes;
                return true;
            case WeaponType.Claws:
                rules = _config.RulesByType.WeaponTypes.Claws;
                return true;
            case WeaponType.Crossbow:
                rules = _config.RulesByType.WeaponTypes.Crossbow;
                return true;
            case WeaponType.Daggers:
                rules = _config.RulesByType.WeaponTypes.Daggers;
                return true;
            case WeaponType.GreatSword:
                rules = _config.RulesByType.WeaponTypes.Greatsword;
                return true;
            case WeaponType.Longbow:
                rules = _config.RulesByType.WeaponTypes.Longbow;
                return true;
            case WeaponType.Mace:
                rules = _config.RulesByType.WeaponTypes.Mace;
                return true;
            case WeaponType.Pistols:
                rules = _config.RulesByType.WeaponTypes.Pistols;
                return true;
            case WeaponType.Scythe:
                rules = _config.RulesByType.WeaponTypes.Reaper;
                return true;
            case WeaponType.Slashers:
                rules = _config.RulesByType.WeaponTypes.Slashers;
                return true;
            case WeaponType.Spear:
                rules = _config.RulesByType.WeaponTypes.Spear;
                return true;
            case WeaponType.Sword:
                rules = _config.RulesByType.WeaponTypes.Sword;
                return true;
            case WeaponType.Twinblades:
                rules = _config.RulesByType.WeaponTypes.Twinblades;
                return true;
            case WeaponType.Whip:
                rules = _config.RulesByType.WeaponTypes.Whip;
                return true;
            default:
                rules = default;
                return false;
        }
    }

    public bool IsValidWeaponSlot(int slotIndex)
    {
        return slotIndex <= _maxSlotIndex;
    }

    public bool TryFindWastedWeaponSlot(Entity character, out int slotIndex)
    {
        for (var i = 0; i <= _maxSlotIndex; i++)
        {
            // todo: get the buffer and check directly
            if (!InventoryUtilities.TryGetItemAtSlot(EntityManager, character, i, out InventoryBuffer itemInSlot))
            {
                LogUtil.LogInfo("Found empty slot");
                slotIndex = i;
                return true;
                // empty slot?
            }
            // todo: check if wasted
        }

        // todo: implement

        // things that are not considered wasted:
        // weapons, except the fishing pole

        // todo: maybe let user configure which items
        // healing potions
        //   Blood rose potion
        //   Blood rose brew
        //   Vermin Salve

        // return an empty slot, before returning a filled slot that's wasted

        slotIndex = default;
        return false;
    }

}
