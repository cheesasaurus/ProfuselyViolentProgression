BuffSystem_Spawn_Server seems to be responsible for actually spawning the buffs.

There's not really a consistent archetype for every query match.
It just checks for Buff and LifeTime and SpawnTag.

LifeTime.Duration is a float, representing the number of seconds. i.e. 5 for 5 seconds.

EntityOwner is the entity to be buffed. At least sometimes; not sure if this is always the case.
Buff.Target might be more reliable? Also not 100% sure, but the naming sure is convincing.

EntityCreator is the entity that's triggering this buff to be spawned.
E.g. An entity with prefabGuid Buff_Waypoint_TravelEnd causes Buff_General_Phasing to be spawned.

## Noteworthy comnponents seen on the entities matched (varies)

- PrefabGUID - the buff to spawn. pretty sure its always there.
- ProjectM.BuffModificationFlagData - contains a `long` flag, to be interpreted as BuffModificationTypes.
  - e.g. Invulnerable, Immaterial, WaypointImpair...
- `ModifyUnitStatBuff_DOTS[Buffer]`
- `SpellModArithmetic[Buffer]`
- `SpellModSetComponent`
