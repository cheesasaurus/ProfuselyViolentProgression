# PalacePrivileges

⚠️WIP⚠️


## Chat Commands

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

### Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 