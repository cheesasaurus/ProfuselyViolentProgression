# Design notes

## Objective

Enable players to safely team up for pvp. Mititgates "insiding" (griefing from clan members).

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
- `.castleprivs grant [building, lockbox, doors.all] player Bilbo`
- `.castleprivs ungrant [doors.notServantLocked] clan`
- `.castleprivs forbid [all] player Gollum` - in case Gollum gets in the clan, don't let him do anything
- `.castleprivs unforbid [jukebox] player Gollum` - i guess it wouldn't hurt to let him play some music
- `.castleprivs check player Bilbo`
- `.castleprivs check clan`

## Misc
- Player always has full privs for their own castle. Cannot grant/revoke privs to self. 
- Each player has a lookup of privileges they've granted. [clan, players[Bilbo, Frodo]]
  - internally, use platformId to identify players, not names.
    - Or serialized entity? not sure if that's safe. 
- Clan privileges default: todo. give access to doors.servantLocked
- Player privileges default: none
- global configuration (admin only)
  - todo
- Checking if an action is allowed:
  - let grantedPrivs be the OR combination of the castle owner's granted privileges
    - the player privileges specified for the actor
    - if the actor is in the same clan as the owner, the clan privileges
  - let forbiddenPrivs be the castle owner's forbidden privileges for the actor
  - `appliedPrivs = grantedPrivs & ~forbiddenPrivs`


### bit flags enums
- general
  - lockbox
  - building
- doors
- servants
