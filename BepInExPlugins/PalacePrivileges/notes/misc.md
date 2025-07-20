# misc notes

- CastleBuilding.CharacterMenuOpenedSystem_Server - something to do with the opening the build menu?
- BuildModeOpenedSystem_Server - something to do with the opening the build menu?

- BuildModeOperations - utility for building-related things?

- GetPlacementResourcesResult - utility for checking building costs / refunds.
- ApplyPlacementResourcesResult - utility for applying building costs / refunds.

- GetPlacementResult - utility for placing tile models?
- ApplyPlacementResult - utility for placing tile models?



- CreateTileModelData - has a CastleHeartConnection. and a CastleTerritoryConnection.

- CurrentTileModelEditing - ecs component found on a player character entity. contains an Entity field called TileModel.
  - doesn't point to anything when placing a new TM




- PlacementTypeBasicFlags - various categories of castle structures (floor, wall, entrance, door...)

- PlacementTypeObjectFlags.Seed
- PlacementTypeObjectFlags.SeedPlanter

- PlacementTypeObjectFlags.Sapling
- PlacementTypeObjectFlags.SaplingPlanter

- PlacementTypeData - contains various flags to configure a placeable thing.
- PlacementData - contains categories of PlacementTypeData for a placeable thing: requirements, restrictions, replaces, attachesTo, MustMatchAllrequiprements

- TileBlob - contains a PlacementData called "AllUsedPlacementFlags"
- TileData - contains a BlobAssetReference<TileBlob>..



- GenerateCastleSystem


- MapZoneCollection
- MapZoneData
- TerrainChunk - chunk coordinates

- MapZoneUtilities.TryGetMapZoneForWorldPos


- CurrentMapZone - ecs component on a User entity. but gets us a world zone, not a map zone?

- CastleTerritory - has a MapZoneId, an Entity `CastleHeart`, and an int `CastleTerritoryIndex`

- CastleHeartHelpers


- TileUtility
- TileWorldUtility


- LineOfSightUtility

- WorldZoneCollection.TryFindZoneForPosition

- RoomUtility

- CastleArenaUtility

- TerrainUtility - can get indexes for tiles and blocks

- SpaceConversion - utility for coordinate/index conversion. world, tile, local, chunk


- ServerGameManager - has a bunch of utility stuff

- ServerGameManager.InterruptCast(Entity character) - TODO: try this out. could be the missing piece for WallopWarpers

