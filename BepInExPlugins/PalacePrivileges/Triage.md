# Triage

## MVP

- [x] Privs: opening / closing doors. (no door coloring)
- [ ] Priv: `build.all` (allow/disallow everything)
- [x] Restrict: Abandoning the castle.
- [x] Restrict: Exposing the castle heart.
- [x] Restrict: Removing blood from the castle heart.
- [ ] Restrict: Relocating the castle. - CastleRebuildConnectEvent?
- [ ] Priv: `arena.zonePainting`
- [ ] Priv: `lockbox` - InteractValidator?
- [x] Priv: `servants.terminate`
- [x] Priv: `servants.gear`
- [ ] Priv: `prisoners.subdue` (moved to mvp for anticheat)
- [ ] Priv: `prisoners.kill` (moved to mvp for anticheat)
- [ ] Priv: `prisoners.extractBlood` (moved to mvp for anticheat)
- [ ] Priv: `prisoners.feedUnsafeFood` (moved to mvp for anticheat)
- [ ] Priv: `prisoners.feedSafeFood` (anticipating to implement alongside feedUnsafeFood)
- [ ] Privs: `tp.red` `tp.yellow` `tp.purple` `tp.blue` `tp.allSmall`
- [ ] Privs: `tp.waygate` `tp.waygateIn` `tp.waygateOut`
- [ ] Priv: `craft.useTreasury` (crafting station AND from inventory. should apply to horse station for elixers.)
- [x] Priv: `servants.rename`
- [x] Priv: `renameStructures`
- [ ] Priv: `redist.edit`
- [ ] Priv: `redist.toggleAutoSend`
- [ ] Priv: `servants.throne`
- [ ] Priv: `craft.toggleRecipes`


## Future features

- [ ] Cooldown: Using key after leaving clan. will make clan history lib first.
- [x] Privs: opening / closing doors, by color
- [ ] webapp with UI to set up privileges, and spit out commands.
  - check how arena does it.
  - maybe a special export string, with a command like `.castlePrivs import asdfklkjasdfjkhasdflongassstringbutnotlongerthanthemessagesizelimit`
  - same hosting strategy as CastleHeartPolice.
  - a chat command to export from the game wouldn't be usable, because the client doesn't have a way to copy/paste from messages.
- [ ] Priv: `craft.accessStations`
- [ ] Priv: `musicbox` - MoodSystem
- [x] Priv: `servants.convert`
- [ ] Priv: `research.t1` `research.t2` `research.t3`
- [ ] Priv: `research.stygian`
- [ ] Priv: `research.useTreasury`
- [ ] Priv: `arena.editRules` (not sure if this is server or clientside)
- [ ] Priv: `arena.startContest` - CastleArenaUtility
- [ ] Priv: `redist.quickSend`
- [ ] Priv: `build.useTreasury`
- [ ] Privs: planting things in garden
- specific privs for building
- replacing damaged walls
- replacing damaged doors - only with a door of the same type?