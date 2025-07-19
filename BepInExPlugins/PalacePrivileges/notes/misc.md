# misc notes

- CastleBuilding.CharacterMenuOpenedSystem_Server - something to do with the opening the build menu?

- BuildModeOperations - utility for building-related things?

- GetPlacementResourcesResult - utility for checking building costs / refunds.
- ApplyPlacementResourcesResult - utility for applying building costs / refunds.

- GetPlacementResult - utility for placing tile models?
- ApplyPlacementResult - utility for placing tile models?



- CreateTileModelData - has a CastleHeartConnection. and a CastleTerritoryConnection.


- CurrentTileModelEditing - ecs component found on a player character entity. contains an Entity field called TileModel.




- PlacementTypeBasicFlags - various categories of castle structures (floor, wall, entrance, door...)

- PlacementTypeObjectFlags.Seed
- PlacementTypeObjectFlags.SeedPlanter

- PlacementTypeObjectFlags.Sapling
- PlacementTypeObjectFlags.SaplingPlanter

- PlacementTypeData - contains various flags to configure a placeable thing.
- PlacementData - contains categories of PlacementTypeData for a placeable thing: requirements, restrictions, replaces, attachesTo, MustMatchAllrequiprements

- TileBlob - contains a PlacementData called "AllUsedPlacementFlags"
- TileData - contains a BlobAssetReference<TileBlob>. Found as an ecs component on a player character entity.
