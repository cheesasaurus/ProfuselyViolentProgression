# Design notes

## Objective

Enable players to safely clan up for pvp. Mititgates "insiding" (griefing from clan members).

## initial thoughts

- only castle owner should be able to remove essence from heart / expose heart / abandon castle
- grantable/revokable privileges that castle owner can give to clanmates
  - building
  -  lockbox access
  - door opening/closing
    - maybe only certain kinds of doors can't be opened/closed. clanmates should be able to use doors at the breach zone to defend the castle. And other doors could be used to set up private areas of the castle that only the owner can access
  - servant management
  - prison cell interaction


## Example commands

- `.castleprivs grant player Bilbo "building|lockbox|doors.all"`
- `.castleprivs ungrant clan "doors.notServantLocked"`
- `.castleprivs forbid player Gollum "all"` - in case Gollum gets in the clan, don't let him do anything
- `.castleprivs unforbid player Gollum "jukebox"` - i guess it wouldn't hurt to let him play some music
- `.castleprivs check player Bilbo`
- `.castleprivs check clan`
- `.castleprivs check`
- `.castleprivs reset` - reset all granted/forbidden privileges
- `.castleprivs list`

Use term `ungrant` instead of `revoke` to hopefully avoid conflation with `forbid`.

## Misc

- Player always has full privs for their own castle. Cannot grant/revoke privs to self. 
- Each player has a lookup of privileges they've granted. [clan, players[Bilbo, Frodo]]
  - internally, use platformId to identify players, not names.
    - Or serialized entity? not sure if that's safe.
- Player privileges default: none
- global configuration (admin only)
  - apply granted privileges outside of clan (true/false)
    - probably do this as a future feature, since it's adding functionality rather than restricting existing functionality
- Checking if an action is allowed:
  - let grantedPrivs be the OR combination of the castle owner's granted privileges
    - the player privileges specified for the actor. only if in same clan, or global settings permit non-clan members privs
    - if the actor is in the same clan as the owner, the clan privileges
  - let forbiddenPrivs be the castle owner's forbidden privileges for the actor
  - `appliedPrivs = grantedPrivs & ~forbiddenPrivs`
- if somebody in the clan tries to do something without privs:
  - show a chat message warning to the actor.
    - e.g. "{ownerName} has not granted you permission"
    - or maybe SCT. todo: see if anything suitable
  - show a chat message warning to the owner, with a snippet to grant the privs to the actor
    - if actor is in clan, privs for clan
    - otherwise privs for player
  - throttle warning messages to the owner for each owner-actor pair



## bit flags enums

- misc (listed to user without the qualifier. ie. just "building", not "misc.building")
  - building - mvp
  - lockbox - mvp
  - musicbox
  - throne
  - research
  - plantSeeds
  - all (used internally. if the user specifies "all", use each enum's all flag)
- craft
  - accessStations - be sure this doesn't interfere with prison cells
  - useTreasury - mvp
  - toggleRecipes
  - all
- doors - OpenDoorSystem
  - servantLocked - mvp
  - notServantLocked - mvp
  - thin - mvp
  - wide - mvp
  - fence
  - palisade
  - thinBasic
  - thinBattlement
  - thinCordial
  - thinVerdant
  - thinRoyal
  - thinPlagueSanctum
  - thinAncientSymphony1
  - thinAncientSymphony2
  - thinNocturnalOpulenceRed,green,blue... - todo: check actual names and colors. one entry per color.
  - thinPrison
  - thinBarrier
  - wideBars
  - widePlanks
  - all
- prisoners
  - subdue
  - kill
  - extractBlood
  - feedSafeFood
  - feedUnSafeFood
  - all
- servants
  - convert - mvp
  - terminate - mvp
  - gear - mvp
  - rename - NameableInteractableSystem
  - all
- tp
  - waygates
  - red - mvp
  - yellow - mvp
  - purple - mvp
  - blue - mvp
  - all
- redist
  - quickSend
  - toggleAutoSend
  - edit (edit connections, increaase priority, decreaase priority, remove connection)
  - all
- arena
  - startContest
  - rules - todo: not sure if this is shared state
  - zonePainting - mvp. (including "clear arena zone")
  - all


## default clan privs

- tp.waygates
- research
- plantSeeds
- doors.notServantLocked
- doors.wide (release the servants to battle!)
- craft.accessStations
- servants.convert
- redist.quickSend
- arena.startContest
