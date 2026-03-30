using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Services;

public class RecipeService(IRecipeRepository recipeRepository, IIngredientRepository ingredientRepository) : IRecipeService
{
  public Task<IEnumerable<Recipe>> GetAllAsync() => recipeRepository.GetAllAsync();

  public Task<Recipe?> GetByIdAsync(Guid id) => recipeRepository.GetByIdAsync(id);

  public async Task<RecipeMutationResult> CreateAsync(CreateRecipeRequest request)
  {
    var validationErrors = await ValidateIngredientsAsync(request.Ingredients);
    if (validationErrors.Count > 0)
    {
      return RecipeMutationResult.ValidationFailed(validationErrors);
    }

    var recipe = MapRecipeRequest(request.Name, request.Servings, request.Ingredients);
    await recipeRepository.AddAsync(recipe);
    return RecipeMutationResult.Success(recipe);
  }

  public async Task<RecipeMutationResult> UpdateAsync(Guid id, UpdateRecipeRequest request)
  {
    var validationErrors = await ValidateIngredientsAsync(request.Ingredients);
    if (validationErrors.Count > 0)
    {
      return RecipeMutationResult.ValidationFailed(validationErrors);
    }

    var recipe = MapRecipeRequest(request.Name, request.Servings, request.Ingredients);
    recipe.Id = id;

    var updated = await recipeRepository.UpdateAsync(recipe);
    return updated == null
      ? RecipeMutationResult.NotFoundResult()
      : RecipeMutationResult.Success(updated);
  }

  public Task<bool> DeleteAsync(Guid id) => recipeRepository.DeleteAsync(id);

  private async Task<Dictionary<string, string[]>> ValidateIngredientsAsync(List<RecipeIngredientDto>? ingredients)
  {
    if (ingredients == null || ingredients.Count == 0)
    {
      return [];
    }

    var errorAccumulator = new Dictionary<string, List<string>>();

    for (var i = 0; i < ingredients.Count; i++)
    {
      var ingredient = ingredients[i];
      if (ingredient.RequiredQuantity <= 0)
      {
        AddError(
          errorAccumulator,
          $"Ingredients[{i}].RequiredQuantity",
          "Required quantity must be greater than zero.");
      }
    }

    var duplicateIngredientIds = ingredients
      .Where(ingredient => ingredient.IngredientId != Guid.Empty)
      .GroupBy(ingredient => ingredient.IngredientId)
      .Where(group => group.Count() > 1)
      .Select(group => group.Key);

    foreach (var ingredientId in duplicateIngredientIds)
    {
      AddError(
        errorAccumulator,
        nameof(RecipeIngredientDto.IngredientId),
        $"Ingredient '{ingredientId}' cannot appear more than once in a recipe.");
    }

    var ingredientIds = ingredients
      .Where(ingredient => ingredient.IngredientId != Guid.Empty)
      .Select(ingredient => ingredient.IngredientId)
      .Distinct()
      .ToList();

    var ingredientChecks = await Task.WhenAll(ingredientIds.Select(async ingredientId => new
    {
      IngredientId = ingredientId,
      Exists = await ingredientRepository.GetByIdAsync(ingredientId) != null
    }));

    foreach (var ingredientId in ingredientChecks.Where(check => !check.Exists).Select(check => check.IngredientId))
    {
      AddError(
        errorAccumulator,
        nameof(RecipeIngredientDto.IngredientId),
        $"Ingredient '{ingredientId}' does not exist.");
    }

    return errorAccumulator.ToDictionary(entry => entry.Key, entry => entry.Value.ToArray());
  }

  private static Recipe MapRecipeRequest(string name, int servings, List<RecipeIngredientDto> ingredients) =>
    new()
    {
      Name = name,
      Servings = servings,
      Ingredients = [.. ingredients.Select(ingredient => new RecipeIngredient
      {
        IngredientId = ingredient.IngredientId,
        RequiredQuantity = ingredient.RequiredQuantity
      })]
    };

  private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
  {
    if (!errors.TryGetValue(key, out var messages))
    {
      messages = [];
      errors[key] = messages;
    }

    messages.Add(message);
  }
}