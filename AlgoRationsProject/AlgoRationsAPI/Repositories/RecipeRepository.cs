using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Repositories;

public class RecipeRepository : IRecipeRepository
{
  private readonly List<Recipe> _recipes = [];

  public Task<IEnumerable<Recipe>> GetAllAsync() =>
    Task.FromResult<IEnumerable<Recipe>>(_recipes);

  public Task<Recipe?> GetByIdAsync(Guid id) =>
    Task.FromResult(_recipes.FirstOrDefault(r => r.Id == id));

  public Task AddAsync(Recipe recipe)
  {
    recipe.Id = Guid.NewGuid();
    _recipes.Add(recipe);
    return Task.CompletedTask;
  }

  public async Task<Recipe?> UpdateAsync(Recipe recipe)
  {
    var existing = await GetByIdAsync(recipe.Id);
    if (existing != null)
    {
      existing.Name = recipe.Name;
      existing.Ingredients = recipe.Ingredients;
      existing.Servings = recipe.Servings;
    }
    return existing;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    var recipe = await GetByIdAsync(id);
    if (recipe != null)
    {
      _recipes.Remove(recipe);
      return true;
    }
    return false;
  }

  public Task<bool> IsIngredientInUseAsync(Guid ingredientId) =>
    Task.FromResult(_recipes.Any(recipe => recipe.Ingredients.Any(ingredient => ingredient.IngredientId == ingredientId)));

  public Task ClearAsync()
  {
    _recipes.Clear();
    return Task.CompletedTask;
  }
}
