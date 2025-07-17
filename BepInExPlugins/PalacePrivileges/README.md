# PalacePrivileges

⚠️WIP⚠️


Safely clan up for PvP!

Restricts what clan members can do in your castles. Grant more privileges via chat commands.


## Installation

This is a server-side mod. (No need to install anything if you're not the server operator.)

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [HookDOTS](https://thunderstore.io/c/v-rising/p/cheesasaurus/HookDOTS_API/).
- Install [VampireCommandFramework](https://thunderstore.io/c/v-rising/p/deca/VampireCommandFramework/).
- Extract `PalacePrivileges.dll` into _`(VRising folder)/BepInEx/plugins`_.


## Restricted actions

Some actions may only be performed by the owner. Clan members cannot be granted privileges for these restricted actions.
- Abandoning the castle.
- Exposing the castle heart.
- Removing blood from the castle heart.

Recently-separated clan members cannot use keys on your castle heart.\
(The exact timeframe can be configured by an admin. Default 48 hours.)


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

### Examples
- You fully trust Bobby.
  - Grant Bobby the `all` privilege.
- You want to let all clan members use the small teleporters.
  - Grant your clan the `tp.allSmall` privilege.
- You want to let your clan members waygate out, but not in.
  - Ungrant your clan the `tp.waygateIn` privilege.
- Timmy keeps putting lame servants in your coffins.
  - Forbid Timmy the `servants.convert` privilege.
  - Other clan members will still be able to convert servants.
- Suzy wants to milk the prisoners.
  - Grant Suzy the `prisoners.extractBlood` and `prisoners.feedSafeFood` privileges.
  - Suzy will not be able to feed them gruel or corrupted fish.
- Billy is a total sleazebag but you have a common enemy.
  - Forbid Billy the `all` privilege and join his clan.
  - Billy will have no privileges in your castle, despite being a clan member.
- Johnny's favorite color is yellow and you made a special room just for him.
  - Block off his room with the yellow-colored door from the Oakveil DLC.
  - Put a servant lock on his door.
  - Grant Johnny the `doors.noctOpYellow` privilege.



### Miscellaneous privileges

`all`
`lockbox`
`musicbox`
`plantSeeds`

### Build privileges
`build.all`

### Craft privileges
`craft.all`
`craft.useTreasury`
`craft.accessStations`
`craft.toggleRecipes`

### Prisoner privileges
`prisoners.all`
`prisoners.subdue`
`prisoners.kill`
`prisoners.extractBlood`
`prisoners.feedSafeFood`
`prisoners.feedUnsafeFood`

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
`doors.noctOp`
`doors.noctOpRed`
`doors.noctOpOrange`
`doors.noctOpYellow`
`doors.noctOpGreen`
`doors.noctOpGreenLight`
`doors.noctOpBlueLight`
`doors.noctOpBlue`
`doors.noctOpPurple`
`doors.noctOpPink`
`doors.noctOpWhite`
`doors.noctOpGrey`
`doors.noctOpBlack`
`doors.prison`
`doors.barrier`
`doors.wideBars`
`doors.widePlanks`


## Chat commands

Players can customize access to their castles via chat commands.

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
- `.castlePrivs set keyClanCooldownHours 48.0`
  - When clan members separate, they have to wait `48` hours before using keys on each other's castles.
  - Could be set to e.g. `0.5` to make the waiting time 30 minutes.
  - Can be set to `0` to remove the waiting time.
- `.castlePrivs set DebugLogRulings true`
  - When enabled, logs details about every ruling made. For debugging purposes.
  - Disabled by default.


## Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 