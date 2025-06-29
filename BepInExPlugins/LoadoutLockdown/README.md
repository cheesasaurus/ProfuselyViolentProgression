# LoadoutLockdown

⚠️WIP⚠️

Restrict (or unrestrict) equipping gear, both in and out of combat. Fully customizable.

### Some example uses
- Allow equipping a fishing pole from your inventory.
- Disallow swapping armour and amulets during pvp combat.
- Forbid disliked weapons from being equipped.
- Limit how many hotbar slots can be used for weapons.
- Remove all vanilla restrictions, and menu-swap any gear during pvp.


## Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [HookDOTS](https://thunderstore.io/c/v-rising/p/cheesasaurus/HookDOTS_API/).
- Extract `LoadoutLockdown.dll` into _`(VRising folder)/BepInEx/plugins`_.


## Configuration

Configuration files are created after the mod runs.

### Main config

The following configuration settings are available in `(VRisingFolder)/BepInEx/config/LoadoutLockdown.cfg`.

- `RulesetFilepath` [default `MyRuleset.jsonc`]: The location of the ruleset file to use for configuration.

### Rulesets

There are 5 initial rulesets found in `(VRisingFolder)/BepInEx/config/LoadoutLockdown/`

- `MyRuleset` - The default ruleset used. Feel free to edit it.
- `Example_Default` - Default ruleset used to initially set up MyRuleset.
- `Example_FishersFantasy` - Mostly same behaviour as the vanilla game, but with fishing pole restrictions lifted.
- `Example_CrutchersCrucible` - "Bans" some overperforming crutch weapons. Imposes tradeoffs with weapon selection.
- `Example_SweatlordsSwag` - Lifts all restrictions. Menu-swap between 15 weapons and 3 amulets if you want.

Any filenames starting with `Example_` are examples and will be overwritten. Don't change these; copy them to your own files.

In case you don't have LoadoutLockdown installed yet, and want to see what a ruleset looks like: [CrutchersCrucible.jsonc](https://github.com/cheesasaurus/ProfuselyViolentProgression/tree/main/BepInExPlugins/LoadoutLockdown/resources/presets/CrutchersCrucible.jsonc)

### Explanation of rules

- `RequiresHotbarSlot` - If true, requires the gear to be in a valid hotbar slot to use it. Not relevant for things with a designated slot, such as a cape.
- `FromMenuDuringPVP` - Allowed values:
  - `AllowSwapIntoWastedSlot` - During pvp, the gear can only be inserted into an empty slot, or take the place of "junk" in the slot. (i.e. a non-weapon in a hotbar slot)
  - `AllowSwapIntoFilledSlot` - During pvp, the gear can be put into any slot regardless of what's already in it.
- `Forbidden` - If true, the gear can **never** be equipped.

### Forbidden gear
A `forbidden` piece of gear is **never** allowed to be equipped.

Broad types of gear can be forbidden (for example, all slashers).\
As well as specific gear pieces, like "The Thousand Storms".

A searchable list of prefabs (for identifying specific gear) can be found [here](https://wiki.vrisingmods.com/prefabs/Item).


If you do decide to forbid gear, I would recommend setting up some kind of exchange system to "cash in" unusable weapon drops from rifts.\
[Penumbra](https://thunderstore.io/c/v-rising/p/zfolmt/Penumbra/) can help with this.

### Weapon Slots

This is a little-known vanilla setting, which LoadoutLockdown overrides.\
Slots in the action bar are counted from left to right. Equipping a weapon which doesnt sit in a valid slot is disallowed.

For example, with `WeaponSlots` set to `3`, any weapons in slots [4, 5, 6, 7, 8] cannot be equipped. Weapons in the main inventory also cannot be equipped.

The `RequiresHotbarSlot` rule can be set to `false` to let a specific type of weapon (such as the fishing pole) bypass this restriction.

### Wasted slots

An empty slot can always be filled from the menu. Junk items (e.g. a pinecone sitting in a weapon slot) can also be swapped out during PvP.\
Forbidden weapons are considered to be junk items.

Prefabs can be added to the `NotWaste` list to prevent them from being swapped out via `AllowSwapIntoWastedSlot`.

If you would like specific items to always be moveable into their slot (e.g. soul shards), add their prefabs to the `AlwaysAllowSwapIntoSlot` list.


## Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 