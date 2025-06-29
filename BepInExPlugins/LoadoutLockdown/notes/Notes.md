# developer notes (todo: remove / refine into documentation)

Game things:
- Equipment
- EquipmentType
- WeaponType


config should be reloadable
- main config
- rulesets

todo: explore how that "No free action bar slots" message works.\
Maybe there's a way to replace it with the "Nope" message.
- SCT_Type

ResponseEventType
SCTTypeCollection

```
{
  "Guid": "7114de17-65b2-4e69-8723-79f8b33b2213",
  "Text": "Nope!"
},
{
  "Guid": "02997d12-30d2-4e3b-af6a-15bf57814f9f",
  "Text": "No"
},
{
  "Guid": "45e3238f-36c1-427c-b21c-7d50cfbd77bc",
  "Text": "I cannot do that"
},
{
  "Guid": "3bf7e066-4e49-4ae4-b7a3-6703b7a15dc1",
  "Text": "Disabled"
},
{
  "Guid": "6c96ffed-7559-4222-8eec-7fef3fc239e7",
  "Text": "Enabled"
},
{
  "Guid": "c562af88-e3c5-42cb-b35c-bf8a86deb67f",
  "Text": "Off"
},
{
  "Guid": "2c2bd7b6-1e88-4f52-ad6f-48e1121e49ec",
  "Text": "On"
},
{
  "Guid": "62a014f0-13d0-409f-b6d3-a90ae96ac0c1",
  "Text": "It's about to explode!"
},
 {
  "Guid": "6a1ac649-16f2-426d-8686-3c7f594bcfcd",
  "Text": "Smack"
},
{
  "Guid": "54d48cbf-6817-42e5-a23f-354ca531c514",
  "Text": "Done"
},
{
  "Guid": "3d998323-8135-42dc-ba3b-8f76a16b0247",
  "Text": "Enemy vampire nearby"
},
{
  "Guid": "0923f448-e438-4a56-992a-4cdf08d66a58",
  "Text": "Off"
},
{
  "Guid": "0b739f61-3a5a-4c1d-a37d-ab1d6b1fa469",
  "Text": "On"
},
{
  "Guid": "5b2caa92-4d58-4b48-9842-38abf52c1844",
  "Text": "Restricted Slot"
},
{
  "Guid": "b87891fd-9f55-45cd-88e3-6b3381968c54",
  "Text": "Golem"
},
{
  "Guid": "8e2de316-22d6-4088-b23f-9084af440171",
  "Text": "No free action bar slots"
},
{
  "Guid": "dea7f940-4a8f-4592-8bb9-5398cde17c67",
  "Text": "<i>'Oink!'</i>"
},
{
  "Guid": "5958f55c-60c3-4e12-b4d2-8d1d5e9f2c66",
  "Text": "<i>'Oh no!'</i>"
},
{
  "Guid": "8a0bca5f-8f27-48ed-a4e0-7e035e974c3c",
  "Text": "Complete"
},
{
  "Guid": "eecdc625-887f-4a5b-86f7-f268cfd7f2a1",
  "Text": "Lock In"
},
{
  "Guid": "d79029a9-efe8-449c-878a-5830bfcdb1ba",
  "Text": "Limited"
},

```


ForbiddenByPrefab: ISet<PrefabGUID>


Things to know for rule check
  - WeaponSlots rule
  - PrefabGUID of new item that will be equipped
  - Type of item that will be equipped
  - PrefabGUID of item in target slot
  - Type of item in target slot

Things we're given
  - Slot index of the inventory
  - Entity of the character OR Entity of the inventory OR NetworkId of the inventory (maybe character)
  - slot index and (NetworkId or Entity) of the other inventory (IsValidTransfer and IsValidItemMove)


InventoryUtilitiesServer.NotActionBarItem(ItemData):bool


The game automatically fills "junk" hotbar slots with a weapon when its right clicked from inventory.
Some things like potions and rats are not considered junk in this case. But we want to allow swapping weapons into those slots. Not super important tho.


EquippableData
  BuffGuid (PrefabGUID) - this seems to identify the weapon moveset, not the actual weapon. e.g. EquipBuff_Weapon_Sword_Ability01 for bone sword. EquipBuff_Weapon_Sword_Ability02 for copper sword. EquipBuff_Weapon_Sword_Ability03 for iron sword.
  EquipmentType (enum)
  WeaponType (enum)
  EquipmentSet (PrefabGUID) - the equipment set bonus associated with the item. I think this is just for armor pieces. e.g. SetBonus_T09_Dracula_Rogue

ItemData
  Entity - todo: check what this is
  ItemTypeGUID (PrefabGUID) - todo
  ItemType ItemType (enum)
  ItemCategory ItemCategory (enum, flags)
  RemoveOnConsume: bool
  


EquipItemSystem doesn't do anything when item picked up from the ground
EquipItemFromInventorySystem doesn't do anything when item picked up from the ground, or when item crafted