using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding.Placement;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class UseTreasuryForBuildingPatch
{
    private const bool SKIP_ORIGINAL_METHOD = false;
    private const bool EXECUTE_ORIGINAL_METHOD = true;

    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;

    /// <remarks>
    /// GetPlacementResourcesResult.HasEnoughResources is called when a player tries to place a new TileModel while building
    /// </remarks>
    [HarmonyPatch(typeof(GetPlacementResourcesResult), nameof(GetPlacementResourcesResult.HasEnoughResources))]
    [HarmonyPrefix]
    public static bool EnforceTreasuryPrivilegesForBuilding(
        ref bool __result,
        ref MapZoneCollection mapZoneCollection,
        PlacementResourcesResult resourcesResult,
        EntityManager entityManager,
        Entity character,
        BuildResourceConsumeType resourceConsumeType
    )
    {
        if (!Core.IsInitialized)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        if (resourceConsumeType is not BuildResourceConsumeType.SharedInventory)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        var ruling = Core.RestrictionService.ValidateAction_BuildUseTreasury(character);
        if (ruling.IsAllowed)
        {
            return EXECUTE_ORIGINAL_METHOD;
        }

        var substitutedResourceConsumeType = BuildResourceConsumeType.LocalInventory;

        __result = GetPlacementResourcesResult.HasEnoughResources(
            mapZoneCollection,
            resourcesResult,
            entityManager,
            character,
            substitutedResourceConsumeType
        );

        if (__result is false)
        {
            Core.NotificationService.NotifyActionDenied(character, ref ruling);
        }

        return SKIP_ORIGINAL_METHOD;
    }
    

}