BuffSystem_Spawn_Server seems to be responsible for actually spawning the buffs.

There's not really a consistent archetype for every query match.
It just checks for Buff and LifeTime and SpawnTag.

## Noteworthy comnponents seen on the entities matched (varies)

- PrefabGUID - the buff to spawn. pretty sure its always there.
- ProjectM.BuffModificationFlagData - contains a `long` flag, to be interpreted as BuffModificationTypes.
  - e.g. Invulnerable, Immaterial, WaypointImpair...
- `ModifyUnitStatBuff_DOTS[Buffer]`
- `SpellModArithmetic[Buffer]`
- `SpellModSetComponent`
