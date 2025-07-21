# Triage

## MVP

- [x] Privs: opening / closing doors. (no door coloring)
- [x] Priv: `build.all` (allow/disallow everything)
- [x] Restrict: Abandoning the castle.
- [x] Restrict: Exposing the castle heart.
- [x] Restrict: Removing blood from the castle heart.
- [x] Restrict: Relocating the castle.
- [x] Priv: `arena.zonePainting`
- [x] Priv: `lockbox`
- [x] Priv: `servants.terminate`
- [x] Priv: `servants.gear`
- [x] Priv: `prisoners.subdue` (moved to mvp for anticheat)
- [x] Priv: `prisoners.kill` (moved to mvp for anticheat)
- [x] Priv: `prisoners.extractBlood` (moved to mvp for anticheat)
- [x] Priv: `prisoners.feedUnsafeFood` (moved to mvp for anticheat)
- [x] Priv: `prisoners.feedSafeFood` (anticipating to implement alongside feedUnsafeFood)
- [x] Privs: `tp.red` `tp.yellow` `tp.purple` `tp.blue` `tp.allSmall`
- [x] Privs: `tp.waygate` `tp.waygateIn` `tp.waygateOut`
- [x] Priv: `servants.rename`
- [x] Priv: `renameStructures`
- [x] Priv: `redist.editRoutes` - be sure to validate the "clear all" thing at the engine itself too
- [x] Priv: `redist.toggleAutoSend`
- [x] Priv: `servants.throne`
- [ ] Priv: `toggleRefinement`


## Important but not feasible

Issue: harmony can't patch generic methods. Which `InventoryUtilities.CanCraftRecipe` happens to be.
Of course we could go up a few levels and implement our own version of the entire thing ourselves... but not worth it.

- [ ] Priv: `craft.useTreasury` (crafting station AND from inventory)
- [ ] Priv: `prison.useTreasury` - prison buttons are techcnically crafting, so same limitation.
- [ ] Priv: `repair.useTreasury`
- [ ] Priv: using treasury for horse perks at stables. Probably doable with StablesUtility.CanAfford...
  - but considering just a coverall `useTreasury` privilege instead of having to set things one-by-one.


## Future features

- [ ] Cooldown: Using key after leaving clan. will make clan history lib first.
- [x] Privs: opening / closing doors, by color
- [x] `Check` command should not show redundant privileges. (e.g. listing all the seeds when sowseed.all is on)
- [ ] webapp with UI to set up privileges, and spit out commands.
  - check how arena does it.
  - maybe a special export string, with a command like `.castlePrivs import asdfklkjasdfjkhasdflongassstringbutnotlongerthanthemessagesizelimit`
  - same hosting strategy as CastleHeartPolice.
  - a chat command to export from the game wouldn't be usable, because the client doesn't have a way to copy/paste from messages.
- [x] Priv: `craft.craftItem`
- [x] Priv: `musicbox` - MoodSystem
- [x] Priv: `servants.convert`
- [x] Priv: `research.t1` `research.t2` `research.t3`
- [x] Priv: `research.stygian`
- [ ] Priv: `research.useTreasury`
- [x] Priv: `arena.useStation`
- [x] Priv: `redist.quickSend`
- [x] Priv: `build.useTreasury`
- [x] Privs: planting things in garden
- [ ] specific privs for building. needs design
- [ ] replacing damaged walls
- [ ] replacing damaged doors - only with a door of the same type?
- [ ] command: `.castlePrivs reset clan`
- [ ] command: `.castlePrivs reset players`