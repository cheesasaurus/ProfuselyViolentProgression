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


EquippableToHotbarRules
    Forbidden
    FromMenuDuringPVP
    RequiresHotbarSlot


EquippableToOwnSlotRules
    Forbidden
    FromMenuDuringPVP

ForbiddenByPrefab: ISet<PrefabGUID>