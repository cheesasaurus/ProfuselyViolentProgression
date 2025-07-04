using System;
using System.Collections.Generic;
using ProfuselyViolentProgression.Core.Utilities;
using ProfuselyViolentProgression.LoadoutLockdown.Config;
using ProjectM;
using ProjectM.Network;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProfuselyViolentProgression.LoadoutLockdown;


internal class LoadoutLockdownService
{
    public static LoadoutLockdownService Instance;

    private LoadoutLockdownConfig _config;
    private EntityManager EntityManager => WorldUtil.Server.EntityManager;
    private EndSimulationEntityCommandBufferSystem EndSimulationEntityCommandBufferSystem => WorldUtil.Server.GetExistingSystemManaged<EndSimulationEntityCommandBufferSystem>();

    // todo: cache things. should be on a service probably
    private static NativeParallelHashMap<PrefabGUID, ItemData> ItemHashLookupMap => WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>().ItemHashLookupMap;
    private static ServerScriptMapper ServerScriptMapper => WorldUtil.Server.GetExistingSystemManaged<ServerScriptMapper>();
    private static ServerRootPrefabCollection ServerRootPrefabCollection => ServerScriptMapper.GetSingleton<ServerRootPrefabCollection>();
    private static NetworkIdLookupMap NetworkIdLookupMap => ServerScriptMapper.GetSingleton<NetworkIdSystem.Singleton>()._NetworkIdLookupMap;

    private int _maxWeaponSlotIndex => _config.WeaponSlots - 1;
    private HashSet<PrefabGUID> _forbiddenByPrefab = new();
    private HashSet<PrefabGUID> _notWaste = new();
    private HashSet<PrefabGUID> _alwaysAllowSwapIntoSlot = new();

    public int WeaponSlots => _config.WeaponSlots;

    public LoadoutLockdownService(LoadoutLockdownConfig config)
    {
        _config = config;
        InitForbiddenByPrefab();
        InitNotWaste();
        InitAlwaysAllowSwapIntoSlot();
    }

    private void InitForbiddenByPrefab()
    {
        foreach (string prefabName in _config.ForbiddenByPrefab)
        {
            if (SpawnablePrefabLookup.TryGetValue(prefabName, out var prefabGUID))
            {
                _forbiddenByPrefab.Add(prefabGUID);
            }
            else
            {
                LogUtil.LogWarning($"Unrecognized prefab in ForbiddenByPrefab: {prefabName}");
            }
        }
    }

    private void InitNotWaste()
    {
        foreach (string prefabName in _config.NotWaste)
        {
            if (SpawnablePrefabLookup.TryGetValue(prefabName, out var prefabGUID))
            {
                _notWaste.Add(prefabGUID);
            }
            else
            {
                LogUtil.LogWarning($"Unrecognized prefab in NotWaste: {prefabName}");
            }
        }
    }

    private void InitAlwaysAllowSwapIntoSlot()
    {
        foreach (string prefabName in _config.AlwaysAllowSwapIntoSlot)
        {
            if (SpawnablePrefabLookup.TryGetValue(prefabName, out var prefabGUID))
            {
                if (_forbiddenByPrefab.Contains(prefabGUID))
                {
                    LogUtil.LogWarning($"{prefabName} was removed from AlwaysAllowSwapIntoSlot, because it is ForbiddenByPrefab.");
                }
                else
                {
                    _alwaysAllowSwapIntoSlot.Add(prefabGUID);
                }
            }
            else
            {
                LogUtil.LogWarning($"Unrecognized prefab in AlwaysAllowSwapIntoSlot: {prefabName}");
            }
        }
    }

    private Il2CppSystem.Collections.Generic.Dictionary<string, PrefabGUID> _spawnablePrefabLookup;
    private Il2CppSystem.Collections.Generic.Dictionary<string, PrefabGUID> SpawnablePrefabLookup
    {
        get
        {
            if (_spawnablePrefabLookup is null)
            {
                var prefabCollectionSystem = WorldUtil.Server.GetExistingSystemManaged<PrefabCollectionSystem>();
                _spawnablePrefabLookup = prefabCollectionSystem.SpawnableNameToPrefabGuidDictionary;
            }
            return _spawnablePrefabLookup;
        }
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
        return _forbiddenByPrefab.Contains(prefabGUID);
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

    private static HashSet<EquipmentType> EquipmentTypesWithDesignatedSlot = [
        EquipmentType.Bag,
        EquipmentType.Cloak,
        EquipmentType.Headgear,
        EquipmentType.MagicSource,
        EquipmentType.Chest,
        EquipmentType.Legs,
        EquipmentType.Gloves,
        EquipmentType.Footgear,
    ];

    public bool HasDesignatedSlot(Entity entity)
    {
        if (!EntityManager.HasComponent<EquippableData>(entity))
        {
            return false;
        }
        var equippableData = EntityManager.GetComponentData<EquippableData>(entity);
        return EquipmentTypesWithDesignatedSlot.Contains(equippableData.EquipmentType);
    }

    public bool IsDesignatedSlotWasted(Entity character, Entity equippableEntity)
    {
        if (!HasDesignatedSlot(equippableEntity))
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

        var entityInSlot = equipment.GetEquipmentEntity(equippableData.EquipmentType)._Entity;
        if (!EntityManager.HasComponent<PrefabGUID>(entityInSlot))
        {
            return false;
        }
        var prefabGUIDInSlot = EntityManager.GetComponentData<PrefabGUID>(entityInSlot);
        if (_notWaste.Contains(prefabGUIDInSlot))
        {
            return false;
        }
        // could potentially compare the gear (e.g. levels), but let's not overcomplicate it for something that nobody is asking for.
        return false;
    }

    public bool CanMenuSwapIntoFilledSlotDuringPVP(Entity entity)
    {
        if (AlwaysAllowSwapIntoSlot(entity))
        {
            return true;
        }

        if (!EntityManager.HasComponent<EquippableData>(entity))
        {
            return false;
        }
        var equippableData = EntityManager.GetComponentData<EquippableData>(entity);

        if (TryFindSwappableFromMenuDuringPVP(equippableData, out var swappableRules))
        {
            return swappableRules.FromMenuDuringPVP == FromMenuDuringPVP.AllowSwapIntoFilledSlot;
        }
        return false;
    }

    public bool CanDirectlyMoveOutOfSlotDuringPVP(Entity item)
    {
        return CanMenuSwapIntoFilledSlotDuringPVP(item);
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
        return slotIndex <= _maxWeaponSlotIndex;
    }

    public bool IsPlayerInventory(Entity inventoryEntity)
    {
        if (TryGetOwnerOfInventory(inventoryEntity, out var owner))
        {
            return EntityManager.HasComponent<PlayerCharacter>(owner);
        }
        return false;
    }

    public bool TryGetOwnerOfInventory(Entity inventoryEntity, out Entity owner)
    {
        if (!EntityManager.HasComponent<InventoryConnection>(inventoryEntity))
        {
            owner = default;
            return false;
        }
        var inventoryConnection = EntityManager.GetComponentData<InventoryConnection>(inventoryEntity);
        owner = inventoryConnection.InventoryOwner;
        return true;
    }

    public bool TryFindWastedWeaponSlot(Entity character, out int slotIndex)
    {
        if (!InventoryUtilities.TryGetInventory(EntityManager, character, out var ibb))
        {
            slotIndex = default;
            return false;
        }

        var hasEmptySlot = false;
        var firstEmptySlotIndex = 0;

        var hasJunkSlot = false;
        var firstJunkSlotIndex = 0;

        for (var i = 0; i <= _maxWeaponSlotIndex; i++)
        {
            if (!hasEmptySlot && ibb[i].Amount == 0)
            {
                hasEmptySlot = true;
                firstEmptySlotIndex = i;
            }

            if (!hasJunkSlot && IsWasteInWeaponSlot(ibb[i]))
            {
                hasJunkSlot = true;
                firstJunkSlotIndex = i;
            }
        }

        if (hasEmptySlot)
        {
            slotIndex = firstEmptySlotIndex;
            return true;
        }
        else if (hasJunkSlot)
        {
            slotIndex = firstJunkSlotIndex;
            return true;
        }
        slotIndex = default;
        return false;
    }

    public bool IsWasteInWeaponSlot(InventoryBuffer inventoryBufferEl)
    {
        var prefabGUID = inventoryBufferEl.ItemType;
        if (_notWaste.Contains(prefabGUID))
        {
            return false;
        }
        if (IsEquipmentForbidden(inventoryBufferEl.ItemEntity._Entity))
        {
            // they can't use that weapon, so let them swap it out.
            return true;
        }

        var itemEntity = inventoryBufferEl.ItemEntity._Entity;
        if (EntityManager.HasComponent<EquippableData>(itemEntity))
        {
            var equippableData = EntityManager.GetComponentData<EquippableData>(itemEntity);
            return equippableData.EquipmentType is not EquipmentType.Weapon
                || equippableData.WeaponType is WeaponType.FishingPole;
        }

        return true;
    }

    public void SwapItemsInSameInventory(Entity character, int slotIndexA, int slotIndexB)
    {
        if (InventoryUtilities.TryGetMainInventoryEntity(EntityManager, character, out var mainInventoryEntity))
        {
            var ibb = EntityManager.GetBuffer<InventoryBuffer>(mainInventoryEntity);
            var temp = ibb[slotIndexA];
            ibb[slotIndexA] = ibb[slotIndexB];
            ibb[slotIndexB] = temp;
        }
    }

    public bool AlwaysAllowSwapIntoSlot(Entity entity)
    {
        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return false;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);
        return _alwaysAllowSwapIntoSlot.Contains(prefabGUID);
    }


    public static AssetGuid SCTMessage_Nope = AssetGuid.FromString("7114de17-65b2-4e69-8723-79f8b33b2213");
    public static AssetGuid SCTMessage_Disabled = AssetGuid.FromString("3bf7e066-4e49-4ae4-b7a3-6703b7a15dc1");
    public static AssetGuid SCTMessage_CannotModifyActionBarWhilePVP = AssetGuid.FromString("1b032d4c-f114-429b-ad7c-43c2cd23262a");
    public static AssetGuid SCTMessage_NoFreeActionBarSlots = AssetGuid.FromString("8e2de316-22d6-4088-b23f-9084af440171");


    public static float3 ColorRed = new float3(255, 0, 0);

    public void CreateSCTMessage(Entity character, AssetGuid messageGuid, float3 color)
    {
        var translation = EntityManager.GetComponentData<Translation>(character);

        ScrollingCombatTextMessage.Create(
            EntityManager,
            EndSimulationEntityCommandBufferSystem.CreateCommandBuffer(),
            messageGuid,
            translation.Value,
            color,
            character
            //value,
            //sct,
            //player.UserEntity
        );
    }

    public void SendMessageEquipmentForbidden(Entity character)
    {
        CreateSCTMessage(character, SCTMessage_Disabled, ColorRed);
    }

    public void SendMessageCannotMenuSwapDuringPVP(Entity character)
    {
        CreateSCTMessage(character, SCTMessage_CannotModifyActionBarWhilePVP, ColorRed);
    }

    public void SendMessageNoFreeWeaponSlots(Entity character)
    {
        CreateSCTMessage(character, SCTMessage_NoFreeActionBarSlots, ColorRed);
    }

    public bool IsValidItemEquip(Entity character, Entity fromInventory, int fromSlotIndex, bool isCosmetic)
    {
        if (isCosmetic)
        {
            return true;
        }

        if (!InventoryUtilities.TryGetItemAtSlot(EntityManager, fromInventory, fromSlotIndex, out InventoryBuffer candidateIB))
        {
            LogUtil.LogWarning("IsValidItemEquip could not find candidateIB");
            return false;
        }
        var candidateItemEntity = candidateIB.ItemEntity._Entity;

        if (IsEquipmentForbidden(candidateItemEntity))
        {
            SendMessageEquipmentForbidden(character);
            return false;
        }

        if (IsEquippableWithoutSlot(candidateItemEntity))
        {
            return true;
        }

        bool isInPvPCombat = NewWeaponEquipmentRestrictionsUtility.IsInPvPCombat(EntityManager, ServerRootPrefabCollection, character);

        if (HasDesignatedSlot(candidateItemEntity))
        {
            var isAllowed = !isInPvPCombat
                || CanMenuSwapIntoFilledSlotDuringPVP(candidateItemEntity)
                || IsDesignatedSlotWasted(character, candidateItemEntity);

            if (!isAllowed)
            {
                SendMessageCannotMenuSwapDuringPVP(character);
            }
            // if __result is true, the game will take care of swapping the equipped item into the slot.
            // but only for things that have their own designated slot
            return isAllowed;
        }

        if (IsValidWeaponSlot(fromSlotIndex))
        {
            return true;
        }

        // NewWeaponEquipmentRestrictionsUtilty.IsValidWeaponEquip has a side effect of swapping the item into a wasted slot,
        // so we mimic that ourselves. But with different rules about what counts as a wasted slot.
        if (TryFindWastedWeaponSlot(character, out var wastedSlotIndex))
        {
            SwapItemsInSameInventory(character, fromSlotIndex, wastedSlotIndex);
            return true;
        }

        SendMessageNoFreeWeaponSlots(character);
        return false;
    }

    public bool IsValidItemDrop(Entity character, Entity fromInventory, int slotIndex)
    {
        if (!InventoryUtilities.TryGetItemAtSlot(EntityManager, fromInventory, slotIndex: slotIndex, out InventoryBuffer itemIB))
        {
            LogUtil.LogWarning("IsValidItemDrop could not find candidateIB");
            return false;
        }

        bool isInPvPCombat = NewWeaponEquipmentRestrictionsUtility.IsInPvPCombat(EntityManager, ServerRootPrefabCollection, character);
        bool isNotDroppingFromWeaponSlot = !IsValidWeaponSlot(slotIndex);

        if (!isInPvPCombat || isNotDroppingFromWeaponSlot || IsWasteInWeaponSlot(itemIB))
        {
            return true;
        }

        SendMessageCannotMenuSwapDuringPVP(character);
        return false;
    }

    public bool IsValidItemDropFromDedicatedSlot(Entity character, EquipmentType equipmentType)
    {
        bool isInPvPCombat = NewWeaponEquipmentRestrictionsUtility.IsInPvPCombat(EntityManager, ServerRootPrefabCollection, character);
        if (!isInPvPCombat)
        {
            return true;
        }

        if (!EntityManager.HasComponent<Equipment>(character))
        {
            LogUtil.LogWarning("IsValidItemDropFromDedicatedSlot failed to find Equipment on character");
            return true;
        }
        var equipment = EntityManager.GetComponentData<Equipment>(character);
        var entityInSlot = equipment.GetEquipmentEntity(equipmentType)._Entity;

        if (CanDirectlyMoveOutOfSlotDuringPVP(entityInSlot))
        {
            return true;
        }

        SendMessageCannotMenuSwapDuringPVP(character);
        return false;
    }

    public void UnEquipForbiddenItemsFromAllPlayerCharacters()
    {
        var query = EntityManager.CreateEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<PlayerCharacter>(),
                ComponentType.ReadWrite<Equipment>(),
            },
            Options = EntityQueryOptions.IncludeDisabledEntities,
        });

        var entities = query.ToEntityArray(Allocator.Temp);
        var playerCharacters = query.ToComponentDataArray<PlayerCharacter>(Allocator.Temp);
        var equipments = query.ToComponentDataArray<Equipment>(Allocator.Temp);
        LogUtil.LogInfo("Unequipping forbidden items from all players...");
        var count = 0;
        for (var i = 0; i < entities.Length; i++)
        {
            count += UnEquipForbiddenItems(entities[i], playerCharacters[i], equipments[i]);
        }
        LogUtil.LogInfo($"Unequipped {count} forbidden items.");
    }

    private static HashSet<EquipmentType> EquipmentTypesToUnequipIfForbidden = [
        EquipmentType.Bag,
        EquipmentType.Cloak,
        EquipmentType.Headgear,
        EquipmentType.MagicSource,
        EquipmentType.Chest,
        EquipmentType.Legs,
        EquipmentType.Gloves,
        EquipmentType.Footgear,
        EquipmentType.Weapon
    ];

    public int UnEquipForbiddenItems(Entity characterEntity, PlayerCharacter playerCharacter, Equipment equipment)
    {
        int count = 0;
        foreach (var equipmentType in EquipmentTypesToUnequipIfForbidden)
        {
            var itemEntity = equipment.GetEquipmentEntity(equipmentType)._Entity;
            if (IsEquipmentForbidden(itemEntity))
            {                
                var unequipped = InventoryUtilitiesServer.TryUnEquipAndAddItem(
                    EntityManager,
                    ItemHashLookupMap,
                    characterEntity,
                    characterEntity,
                    1,
                    itemEntity,
                    out bool addedToInventory
                );
                if (unequipped)
                {
                    EntityManager.TryGetComponentData<PrefabGUID>(itemEntity, out var prefabGUID);
                    LogUtil.LogInfo($"  Unequipped forbidden {equipmentType} from player {playerCharacter.Name}: {DebugUtil.LookupPrefabName(prefabGUID)}");
                    count++;
                }
                else
                {
                    LogUtil.LogWarning($"  Failed to unequip forbidden {equipmentType} from player {playerCharacter.Name}");
                }
            }
        }
        return count;
    }

}
