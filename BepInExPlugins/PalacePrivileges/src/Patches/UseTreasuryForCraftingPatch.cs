using HarmonyLib;
using ProfuselyViolentProgression.Core.Utilities;
using ProjectM;
using ProjectM.CastleBuilding.Placement;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;

namespace ProfuselyViolentProgression.PalacePrivileges.Patches;

[HarmonyPatch]
public unsafe class UseTreasuryForCraftingPatch
{
    private const bool SKIP_ORIGINAL_METHOD = false;
    private const bool EXECUTE_ORIGINAL_METHOD = true;

    private static EntityManager _entityManager = WorldUtil.Game.EntityManager;

    // todo: figure out how to patch generic methods
    // harmonyx/harmony don't really support it.
    // harmonyx is built on top of monomod, which also doesn't support it?
    // https://harmony.pardeike.net/articles/patching-edgecases.html#generics
    // https://github.com/pardeike/Harmony/issues/201
    // https://github.com/pardeike/Harmony/issues/121


    // error: The given generic instantiation was invalid


    /*

    /// <remarks>
    /// InventoryUtilities.CanCraftRecipe is called when... TODO
    /// I suspect it would be a coverall for everything besides building. Repairing gear, horse perks, crafting stations, prison,... and so on
    /// </remarks>
    [HarmonyPatch(typeof(InventoryUtilities), nameof(InventoryUtilities.CanCraftRecipe))]
    [HarmonyPrefix]
    public unsafe static bool EnforceTreasuryPrivilegesForCrafting_Prefix<TManager>(
        ref bool __result,
        ref NativeParallelHashMap<PrefabGUID, RecipeData> recipeHashLookupMap,
        //TManager manager,
        Entity inventoryTarget,
        PrefabGUID recipeGuid,
        float resourceMultiplier
    )
    {
        // todo

        return EXECUTE_ORIGINAL_METHOD;
    }

    [HarmonyPatch(typeof(InventoryUtilities), nameof(InventoryUtilities.CanCraftRecipe))]
    [HarmonyPostfix]
    public unsafe static void EnforceTreasuryPrivilegesForCrafting_Postfix<TManager>(
        ref bool __result,
        ref NativeParallelHashMap<PrefabGUID, RecipeData> recipeHashLookupMap,
        //TManager manager,
        Entity inventoryTarget,
        PrefabGUID recipeGuid,
        float resourceMultiplier
    )
    {
        // todo
        var prefabInfo = DebugUtil.LookupPrefabName(recipeGuid);
        LogUtil.LogDebug($"checked CanCraftRecipe for {prefabInfo}");
    }
    
    */

}