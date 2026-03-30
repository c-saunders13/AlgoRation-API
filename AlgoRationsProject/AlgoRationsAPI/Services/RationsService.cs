using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Services;

public class RationsService(IIngredientRepository ingredientRepository, IRecipeRepository recipeRepository) : IRationsService
{
  public async Task<RationsResult> CalculateMaxPeopleFedAsync()
  {
    var allIngredients = (await ingredientRepository.GetAllAsync()).ToList();
    var availableIngredients = allIngredients.ToDictionary(i => i.Id, i => i.AvailableQuantity);

    var recipes = (await recipeRepository.GetAllAsync())
      .Where(recipe => IsRecipeValid(recipe, availableIngredients))
      .OrderByDescending(recipe => recipe.Servings)
      .ThenBy(recipe => recipe.Id)
      .ToList();

    if (recipes.Count == 0)
    {
      return new RationsResult(0, [], [.. allIngredients.Select(i => new Ingredient
      {
        Id = i.Id,
        Name = i.Name,
        AvailableQuantity = i.AvailableQuantity
      })]);
    }

    var currentCounts = new int[recipes.Count];
    var bestCounts = new int[recipes.Count];
    var bestPeopleFed = 0;

    SearchOptimalRations(
      index: 0,
      recipes,
      availableIngredients,
      currentCounts,
      currentPeopleFed: 0,
      bestCounts,
      ref bestPeopleFed
    );

    var breakdown = new List<RecipeRationBreakdown>();
    var leftoverQuantities = allIngredients.ToDictionary(i => i.Id, i => i.AvailableQuantity);

    for (var i = 0; i < recipes.Count; i++)
    {
      var servingsMade = bestCounts[i];
      if (servingsMade > 0)
      {
        foreach (var ingredient in recipes[i].Ingredients)
        {
          leftoverQuantities[ingredient.IngredientId] -= ingredient.RequiredQuantity * servingsMade;
        }
      }

      if (servingsMade == 0)
      {
        continue;
      }

      breakdown.Add(new RecipeRationBreakdown
      {
        RecipeId = recipes[i].Id,
        RecipeName = recipes[i].Name,
        ServingsMade = servingsMade,
        PeopleFed = servingsMade * recipes[i].Servings
      });
    }

    var leftoverIngredients = allIngredients.Select(i => new Ingredient
    {
      Id = i.Id,
      Name = i.Name,
      AvailableQuantity = leftoverQuantities[i.Id]
    }).ToList();

    return new RationsResult(bestPeopleFed, breakdown, leftoverIngredients);
  }

  private static bool IsRecipeValid(Recipe recipe, Dictionary<Guid, int> availableIngredients)
  {
    if (recipe.Ingredients.Count == 0 || recipe.Servings <= 0)
    {
      return false;
    }

    foreach (var ingredient in recipe.Ingredients)
    {
      if (ingredient.RequiredQuantity <= 0)
      {
        return false;
      }

      if (!availableIngredients.ContainsKey(ingredient.IngredientId))
      {
        return false;
      }
    }

    return true;
  }

  private static int GetMaxServings(Recipe recipe, Dictionary<Guid, int> availableIngredients)
  {
    var maxServings = int.MaxValue;
    foreach (var ingredient in recipe.Ingredients)
    {
      var availableQuantity = availableIngredients[ingredient.IngredientId];

      var possibleServings = availableQuantity / ingredient.RequiredQuantity;
      if (possibleServings < maxServings)
      {
        maxServings = possibleServings;
      }
    }

    return maxServings;
  }

  private static void SearchOptimalRations(
    int index,
    IReadOnlyList<Recipe> recipes,
    Dictionary<Guid, int> availableIngredients,
    int[] currentCounts,
    int currentPeopleFed,
    int[] bestCounts,
    ref int bestPeopleFed)
  {
    if (index == recipes.Count)
    {
      if (currentPeopleFed > bestPeopleFed)
      {
        bestPeopleFed = currentPeopleFed;
        Array.Copy(currentCounts, bestCounts, currentCounts.Length);
      }

      return;
    }

    var upperBound = currentPeopleFed;
    for (var i = index; i < recipes.Count; i++)
    {
      upperBound += recipes[i].Servings * GetMaxServings(recipes[i], availableIngredients);
    }

    if (upperBound <= bestPeopleFed)
    {
      return;
    }

    var recipe = recipes[index];
    var maxForRecipe = GetMaxServings(recipe, availableIngredients);

    for (var servings = maxForRecipe; servings >= 0; servings--)
    {
      currentCounts[index] = servings;
      var consumed = new List<(Guid ingredientId, int quantity)>();

      if (servings > 0)
      {
        foreach (var ingredient in recipe.Ingredients)
        {
          var quantity = ingredient.RequiredQuantity * servings;
          availableIngredients[ingredient.IngredientId] -= quantity;
          consumed.Add((ingredient.IngredientId, quantity));
        }
      }

      SearchOptimalRations(
        index + 1,
        recipes,
        availableIngredients,
        currentCounts,
        currentPeopleFed + (servings * recipe.Servings),
        bestCounts,
        ref bestPeopleFed
      );

      foreach (var (ingredientId, quantity) in consumed)
      {
        availableIngredients[ingredientId] += quantity;
      }
    }

    currentCounts[index] = 0;
  }
}
