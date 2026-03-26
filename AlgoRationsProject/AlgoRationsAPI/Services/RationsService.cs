using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Services;

public class RationsService(IIngredientRepository ingredientRepository, IRecipeRepository recipeRepository) : IRationsService
{
  public RationsResult CalculateMaxPeopleFed()
  {
    var breakdownDictionary = new Dictionary<Guid, RecipeRationBreakdown>();
    var availableIngredients = ingredientRepository.GetAll()
      .ToDictionary(i => i.Id, i => i.AvailableQuantity);

    var recipes = recipeRepository.GetAll().ToList();
    recipes.Sort((a, b) => b.Servings.CompareTo(a.Servings));

    var peopleFed = 0;
    foreach (var recipe in recipes)
    {
      var maxServings = GetMaxServings(recipe, availableIngredients);
      if (maxServings == 0)
      {
        continue;
      }

      peopleFed += maxServings * recipe.Servings;
      if (breakdownDictionary.TryGetValue(recipe.Id, out RecipeRationBreakdown? value))
      {
        breakdownDictionary[recipe.Id].ServingsMade += maxServings;
        breakdownDictionary[recipe.Id].PeopleFed += maxServings * recipe.Servings;
      }
      else
      {
        breakdownDictionary[recipe.Id] = new RecipeRationBreakdown
        {
          RecipeId = recipe.Id,
          RecipeName = recipe.Name,
          ServingsMade = maxServings,
          PeopleFed = maxServings * recipe.Servings
        };
      }

      foreach (var ingredient in recipe.Ingredients)
      {
        availableIngredients[ingredient.IngredientId] -= ingredient.RequiredQuantity * maxServings;
      }
    }

    return new RationsResult(peopleFed, breakdownDictionary.Values.ToList());
  }

  private static int GetMaxServings(Recipe a, Dictionary<Guid, int> availableIngredients)
  {
    var maxServings = int.MaxValue;
    foreach (var ingredient in a.Ingredients)
    {
      if (!availableIngredients.TryGetValue(ingredient.IngredientId, out var availableQuantity))
      {
        return 0;
      }

      var possibleServings = availableQuantity / ingredient.RequiredQuantity;
      if (possibleServings < maxServings)
      {
        maxServings = possibleServings;
      }
    }

    return maxServings;
  }
}
