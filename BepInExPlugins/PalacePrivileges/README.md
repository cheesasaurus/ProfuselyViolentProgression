# PalacePrivileges

⚠️WIP⚠️


Safely clan up for PvP!

Restricts what clan members can do in your castle. Grant more privileges via chat commands.


## Installation

This is a server-side mod. (No need to install anything if you're not the server operator.)

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [HookDOTS](https://thunderstore.io/c/v-rising/p/cheesasaurus/HookDOTS_API/).
- Install [VampireCommandFramework](https://thunderstore.io/c/v-rising/p/deca/VampireCommandFramework/).
- Extract `PalacePrivileges.dll` into _`(VRising folder)/BepInEx/plugins`_.


## Restricted actions

Some actions may only be performed by the castle owner. Clan members cannot be granted privileges for these restricted actions.
- Abandoning the castle.
- Exposing the castle heart.
- Removing blood from the castle heart.
- Relocating the castle.

Recently-separated clan members cannot use keys on your castle heart.\
(The exact timeframe can be configured by an admin. Default 48 hours.)


## Unrestricted actions

If an enemy could do it, your clan can do it too. Looting boxes, harvesting the garden, etc.\
Put some thought into how you build your castle!


## Clan privileges

By default, clan members will have the following privileges:
  - Opening/closing doors without servant locks.
    - To restrict areas of your castle, put them behind servant-locked doors.
  - Opening/closing portcullises (the 2-tile-wide doors). Even with servant locks.
    - Getting raided? Release the Kraken!
  - Using waygates to teleport in/out.
  - Using crafting stations / refineries.
  - Using research tables and the stygian altar.
  - Converting new servants.
  - Starting arena contests.
  - Sow seeds (not saplings) in the garden.

Notable privileges they do NOT have:
- Building.
- Lockbox access.
- Using resources from the treasury to craft.
- Using the small teleporters.
- Messing with existing servants.
- Messing with existing prisoners.
- Messing with arena layouts.
- Messing with redistribution networks.

In case castle defenses become disabled (enemy used a key), they will be able to:
- Use any door, just like everyone else.
- Use small teleporters, just like everyone else.


## Privileges

Privileges can be granted / ungranted / forbidden.
- `Grant` - Grant privileges to your clan, or to a specific player in your clan.
- `Ungrant` - Revoke privileges granted to your clan, or to a specific player.
- `Forbid` - Overrule clan privileges for a specific player.

Player privileges only apply while you're in the same clan. But they can be set in advance.

This is all done via chat commands...


### Examples

- You fully trust Bobby.
  - `.castlePrivs grant player Bobby "all"`
- Let clan members use the small teleporters.
  - `.castlePrivs grant clan "tp.allSmall"`
- Let clan members waygate out, but not in.
  - `.castlePrivs ungrant clan "tp.waygateIn"`
- Put some servant-locked doors at the entrance for clan members to use.
  - `.castlePrivs grant clan "doors.battlement"`
  - Clan members will now be able to use all "Battlement Gate" doors. Even with servant locks.  
- Billy is a known scoundrel, but you want to team up for a bit of mayhem.
  - `.castlePrivs forbid player Billy "all"`
  - Join his clan.
  - Billy will have no privileges in your castle, despite being a clan member.
- Suzy wants to use the prison.
  - `.castlePrivs grant player Suzy "prison.extractBlood prison.feedSafeFood"`.
  - Suzy will not be able to feed prisoners gruel or corrupted fish.

### Color-coded doors example
- Johnny's favorite color is yellow and you made a special room just for him.
  - Block off his room with the door from the Oakveil DLC.
  - Put a servant lock on his door.
  - Grant Johnny the `doors.dyedYellow` privilege.
  - Dye the door yellow.
  - Now, only you and Johnny will have access to this room!

#### How to dye doors
- Can only be done with the Oakveil DLC door.
- Can be done when placing the door using the color swatch.
- Can also be done after placing the door:
  1. Open the build menu
  2. Mouse over the placed door and hold `Ctrl`
  3. Choose the desired color from the color wheel.


### Miscellaneous privileges
`all`
`lockbox`
`musicbox`
`renameStructures`

### Build privileges
`build.all`
`build.useTreasury`

### Craft privileges
`craft.all`
`craft.useTreasury`
`craft.accessStations`
`craft.toggleRecipes`

### Prison privileges
`prison.all`
`prison.subdue`
`prison.kill`
`prison.extractBlood`
`prison.feedSafeFood`
`prison.feedUnsafeFood`
`prison.useTreasury`

### Servant privileges
`servants.all`
`servants.convert`
`servants.terminate`
`servants.gear`
`servants.rename`
`servants.throne`

### Teleporter privileges
`tp.all`
`tp.waygate`
`tp.waygateOut`
`tp.waygateIn`
`tp.red`
`tp.yellow`
`tp.purple`
`tp.blue`
`tp.allSmall`

### Redistribution privileges
`redist.all`
`redist.quickSend`
`redist.toggleAutoSend`
`redist.edit`

### Research privileges
`research.all`
`research.t1`
`research.t2`
`research.t3`
`research.stygian`
`research.useTreasury`

### Arena privileges
`arena.all`
`arena.startContest`
`arena.editRules`
`arena.zonePainting`

### Door privileges
`doors.all`
`doors.servantLocked`
`doors.notServantLocked`
`doors.thin`
`doors.wide`
`doors.fence`
`doors.palisade`
`doors.basic`
`doors.battlement`
`doors.cordial`
`doors.verdant`
`doors.royal`
`doors.plagueSanctum`
`doors.ancientSymphony1`
`doors.ancientSymphony2`
`doors.nocturnalOpulence`
`doors.prison`
`doors.barrier`
`doors.wideBars`
`doors.widePlanks`
`doors.dyedRed`
`doors.dyedOrange`
`doors.dyedYellow`
`doors.dyedGreen`
`doors.dyedMintGreen`
`doors.dyedCyan`
`doors.dyedBlue`
`doors.dyedPurple`
`doors.dyedPink`
`doors.dyedWhite`
`doors.dyedGrey`
`doors.dyedBlack`

### Garden privileges
`sowSeed.all`
`sowSeed.bloodRose`
`sowSeed.fireBlossom`
`sowSeed.snowFlower`
`sowSeed.hellsClarion`
`sowSeed.mourningLily`
`sowSeed.sunflower`
`sowSeed.plagueBrier`
`sowSeed.grapes`
`sowSeed.corruptedFlower`
`sowSeed.bleedingHeart`
`sowSeed.ghostShroom`
`sowSeed.trippyShroom`
`sowSeed.cotton`
`sowSeed.thistle`

### Arbory privileges
`plantTree.all`
`plantTree.pine`
`plantTree.cypress`
`plantTree.aspen`
`plantTree.aspenAutumn`
`plantTree.birch`
`plantTree.birchAutumn`
`plantTree.apple`
`plantTree.cursed`
`plantTree.gloomy`
`plantTree.cherry`
`plantTree.cherryWhite`
`plantTree.oak`


## Chat commands

- `.castlePrivs list`
  - List all privilege categories, and some misc castle privileges.
- `.castlePrivs list doors`
  - List all possible castle privileges in category `doors`.
- `.castlePrivs grant clan "build.all doors.all"`
  - Grant castle privileges to your clan.
- `.castlePrivs ungrant clan "build.all doors.all"`
  - Revoke castle privileges granted to your clan.
- `.castlePrivs grant player Bilbo "build.all doors.all"`
  - Grant extra castle privileges to the player named `Bilbo`.
  - Privileges only apply while they are in your clan.
  - Granting forbidden privileges will also unforbid them.
- `.castlePrivs ungrant player Bilbo "build.all doors.all"`
  - Revoke extra castle privileges granted to the player named `Bilbo`.
- `.castlePrivs forbid player Gollum "build.all doors.all"`
  - Forbid the player named `Gollum` from getting specific castle privileges while in your clan.
  - Forbidding granted privileges will also ungrant them.
- `.castlePrivs unforbid player Gollum "build.all doors.all"`
  - Unforbid the player named `Gollum` from getting specific castle privileges while in your clan.
- `.castlePrivs reset`
  - Reset castle privileges granted/forbidden to others.
- `.castlePrivs check`
  - List players to whom you've granted/forbade privileges.
- `.castlePrivs check clan`
  - Check castle privileges granted to your clan.
- `.castlePrivs check player Bilbo`
  - Check castle privileges granted/forbidden to the player named `Bilbo`.
- `.castlePrivs settings`
  - Check global settings.


## Admin chat commands

- `.castlePrivs set KeyClanCooldownHours 48.0`
  - When clan members separate, they must wait `48` hours before using keys on each other's castles.
  - Could be set to `0.5` to make the waiting time 30 minutes.
  - Could be set to `0` to remove the waiting time.
- `.castlePrivs set DebugLogRulings true`
  - When enabled, logs details about every ruling made. For debugging purposes.
  - Disabled by default.


## Anticheat

All privilege checks include validation for clan membership. Lockpickers begone!


## Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 