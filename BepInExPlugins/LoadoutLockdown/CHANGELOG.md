# 1.3.0
- When deciding if a weapon slot is wasted, shattered and broken weapons will be treated as junk.
- Bugfix: In some circumstances shattered or broken weapons could be equipped.

# 1.2.0
- Bugfix: Weapons couldn't be unequipped in PVP combat.
- Added `UnEquipAction` option for rulesets. Allowed values:
  - `NotRestricted` - Default. No restrictions on the "unequip action".
  - `DisallowDuringPvpCombat` - The "unequip action" cannot be performed during PVP combat.
  - `DisallowDuringAnyCombat` - The "unequip action" cannot be performed during PVP or PVE combat.

# 1.1.0
- Moving an equipped weapon out of a weapon slot will unequip it. (unless its configured to not require a hotbar slot)
- Added `ApplyPvpMenuSwapRulesToPVE` option for rulesets. Can be enabled to make PVP menu-swapping rules apply to PVE combat too.
- Fixed an issue where trying to equip a weapon from the inventory would move it into a weapon slot, but not equip it.
- Added `LogRulings` option to main config. When enabled, logs details about every ruling made.

# 1.0.0
- Initial release