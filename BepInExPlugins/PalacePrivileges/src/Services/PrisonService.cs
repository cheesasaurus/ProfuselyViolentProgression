using System.Collections.Generic;
using Stunlock.Core;

namespace ProfuselyViolentProgression.PalacePrivileges.Services;

// knows about prison things
public class PrisonService
{
    private Dictionary<PrefabGUID, PrisonRecipeCategory> _recipeCategoryLookup = [];

    public PrisonService()
    {
        CategorizeRecipes();
    }

    public enum PrisonRecipeCategory
    {
        Unknown,
        ExtractBlood,
        SafeFood,
        UnsafeFood,
    }

    public PrisonRecipeCategory DetermineRecipeCategory(PrefabGUID recipePrefabGuid)
    {
        if (!_recipeCategoryLookup.TryGetValue(recipePrefabGuid, out var recipeCategory))
        {
            return PrisonRecipeCategory.Unknown;
        }
        return recipeCategory;
    }

    private void CategorizeRecipes()
    {
        _recipeCategoryLookup = new()
        {
            { PrefabGuids.Recipe_Consumable_PrisonPotion, PrisonRecipeCategory.ExtractBlood },
            { PrefabGuids.Recipe_Consumable_PrisonPotion_Bloodwine, PrisonRecipeCategory.ExtractBlood },
            { PrefabGuids.Recipe_Misc_ExtractEssencePrisoner, PrisonRecipeCategory.ExtractBlood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Rat, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_FatGoby, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_RainbowTrout, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_FierceStinger, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_TwilightSnapper, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_SageFish, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_BloodSnapper, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_SwampDweller, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_GoldenRiverBass, PrisonRecipeCategory.SafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_Fish_Corrupted, PrisonRecipeCategory.UnsafeFood },
            { PrefabGuids.Recipe_Misc_FeedPrisoner_IrradiantGruel, PrisonRecipeCategory.UnsafeFood },
        };
    }    

}