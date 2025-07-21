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


- InteractValidateAndStopSystemServer


- ServerGameManager - has a bunch of utility stuff

- ServerGameManager.InterruptCast(Entity character)

- InteractWithPrisonerSystem
- InteractWithPrisonerEvent
- EventHelper.PrisonInteraction
  - Imprison - happens when placing prisoner into cell
  - Charm - pulling prisoner out of cell with charm button
  - Kill - pressed kill button at cell

- ImprisonedBuffSystem - for dusk callers?


- StartCraftingSystem - start crafting at a station
- StartCharacterCraftingSystem - start crafting on the go (crafting tab in the character menu)

- ResponseEventType.Craft_Not_Enough_Resources

- InventoryUtilities.CanCraftRecipe

- ItemCost

- RepairItemSystem

- StablesPerk.Cost
- StablesUtility.CanAfford
- StablesUtility.TryPay

- CostData

- HUD.ResearchGroupEntry
- HUD.WorkstationRecipeGridSelectionEntry
- UI.DurabilityInfo

- DiscoverCategoryCostBuffer
- DiscoverCostBuffer

- ShatteredItemRepairCost

- CastleHeartCost
- CastleHeartLevelBlobData - contains arrays of CastleHeartCost



- CastleArenaBoundsSystem
- CastleArenaStationDestroySystem
- CastleArenaBuildPreviewSystem (clientside I think)

- CastleArenaBounds
- CastleArenaBlock
- PlannedArenaBlock

- ArenaSyncUtility

- CastleArenaBlockOperationEventSystem


- InventoryRouteEventSystem
  - adding a connection
  - removing a connection
  - re-ordering
  - NOT quick send
  - NOT clear all

- InventoryRouteStationEventSystem
  - clear all button is pressed at redistribution engine

- InventoryRouteDestroySystem - does the actual destroying of routes?

- InventoryRouteSetAutoTransferEventSystem
  - toggle auto send

- InventoryRouteEventSystem
  - manually quick send
  - NOT automatically sending things via processing

- InventoryRouteSetAutoTransferEvent - toggle for auto send
- InventoryRouteModeEvent - toggle for something?
- InventoryRouteClearEvent - clear all connections


