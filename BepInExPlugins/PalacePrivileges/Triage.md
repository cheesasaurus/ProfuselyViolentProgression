# Triage

## MVP

- [x] Privs: opening / closing doors. (no door coloring)
- [x] Priv: `build.all` (allow/disallow everything)
- [x] Restrict: Abandoning the castle.
- [x] Restrict: Exposing the castle heart.
- [x] Restrict: Removing blood from the castle heart.
- [x] Restrict: Relocating the castle.
- [ ] Priv: `arena.zonePainting`
- [x] Priv: `lockbox`
- [x] Priv: `servants.terminate`
- [x] Priv: `servants.gear`
- [x] Priv: `prisoners.subdue` (moved to mvp for anticheat)
- [x] Priv: `prisoners.kill` (moved to mvp for anticheat)
- [ ] Priv: `prisoners.extractBlood` (moved to mvp for anticheat)
- [ ] Priv: `prisoners.feedUnsafeFood` (moved to mvp for anticheat)
- [ ] Priv: `prisoners.feedSafeFood` (anticipating to implement alongside feedUnsafeFood)
- [x] Privs: `tp.red` `tp.yellow` `tp.purple` `tp.blue` `tp.allSmall`
- [x] Privs: `tp.waygate` `tp.waygateIn` `tp.waygateOut`
- [ ] Priv: `craft.useTreasury` (crafting station AND from inventory. should apply to horse station for elixers.)
- [x] Priv: `servants.rename`
- [x] Priv: `renameStructures`
- [ ] Priv: `redist.edit` - be sure to validate the "clear all" thing at the engine itself too
- [ ] Priv: `redist.toggleAutoSend`
- [x] Priv: `servants.throne`
- [ ] Priv: `craft.toggleRecipes`


## Future features

- [ ] Cooldown: Using key after leaving clan. will make clan history lib first.
- [x] Privs: opening / closing doors, by color
- [x] `Check` command should not show redundant privileges. (e.g. listing all the seeds when sowseed.all is on)
- [ ] webapp with UI to set up privileges, and spit out commands.
  - check how arena does it.
  - maybe a special export string, with a command like `.castlePrivs import asdfklkjasdfjkhasdflongassstringbutnotlongerthanthemessagesizelimit`
  - same hosting strategy as CastleHeartPolice.
  - a chat command to export from the game wouldn't be usable, because the client doesn't have a way to copy/paste from messages.
- [ ] Priv: `craft.craft`
- [x] Priv: `musicbox` - MoodSystem
- [x] Priv: `servants.convert`
- [x] Priv: `research.t1` `research.t2` `research.t3`
- [x] Priv: `research.stygian`
- [ ] Priv: `research.useTreasury`
- [x] Priv: `arena.useStation`
- [ ] Priv: `redist.quickSend`
- [x] Priv: `build.useTreasury`
- [x] Privs: planting things in garden
- [ ] specific privs for building. needs design
- [ ] replacing damaged walls
- [ ] replacing damaged doors - only with a door of the same type?
- [ ] command: `.castlePrivs reset clan`
- [ ] command: `.castlePrivs reset players`