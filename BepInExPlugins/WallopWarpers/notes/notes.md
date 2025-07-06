the raid forge mod patches TeleportationRequestSystem
https://github.com/Darreans/RaidForge/blob/main/Patches/TeleportPatches.cs


but I would rather disable interaction with waygates completely while in combat

they should also be kicked out of the interaction if put into combat while interacting/looking at map.

The vanilla game does this for the little castle teleporters already.
Although that might be a client-side thing.

```
{
    "Guid": "d181286d-5bbc-481f-b5aa-3a104dda868f",
    "Text": "Can't teleport while in combat with another vampire"
},
```

With any luck, maybe there is some component that can be slapped on to the relevant waygate entity to block pvp interaction.
Start by investigating the little castle teleporters.


player character entities have a BuffableFlagState.
Enabling the bit for BuffModificationTypes.WaypointImpair prevents the interact prompt from showing in the UI.
Also prevents actual interaction.
This gets switched off by various things. E.g. dashing, shapeshifting... probably a ton of stuff.


A castle heart entity has a CastleTeleporterElement buffer, which references all the little teleporters.
The entities for the little teleporters have CastleTeleporterComponent components.


InteractAbilityBuffer looks suspicious. But lots of blob/prefab string stuff here. not sure what to do with it.


 AB_Interact_UseWaypoint_Blocked - This opens the waygate map when buff applied.



other ideas for WallopWarpers:
- changes to protection after warping in
  - longer timer, and can interact with the waygate to teleport away
- debuff to pvp damage dealt to other players after recently teleporting in