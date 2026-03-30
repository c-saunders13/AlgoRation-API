using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Repositories;

public class IngredientRepository(IRecipeRepository recipeRepository) : IIngredientRepository
{
  private readonly List<Ingredient> _ingredients = [];

  public Task<IEnumerable<Ingredient>> GetAllAsync() =>
    Task.FromResult<IEnumerable<Ingredient>>(_ingredients);

  public Task<Ingredient?> GetByIdAsync(Guid id) =>
    Task.FromResult(_ingredients.FirstOrDefault(i => i.Id == id));

  public Task<Ingredient> AddAsync(Ingredient ingredient)
  {
    ingredient.Id = Guid.NewGuid();
    _ingredients.Add(ingredient);
    return Task.FromResult(ingredient);
  }

  public async Task<Ingredient?> UpdateAsync(Ingredient ingredient)
  {
    var existing = await GetByIdAsync(ingredient.Id);
    if (existing != null)
    {
      existing.Name = ingredient.Name;
      existing.AvailableQuantity = ingredient.AvailableQuantity;
    }
    return existing;
  }

  public async Task<IngredientDeleteResult> DeleteAsync(Guid id)
  {
    var ingredient = await GetByIdAsync(id);
    if (ingredient == null)
    {
      return IngredientDeleteResult.NotFound;
    }

    if (await recipeRepository.IsIngredientInUseAsync(id))
    {
      return IngredientDeleteResult.InUse;
    }

    _ingredients.Remove(ingredient);
    return IngredientDeleteResult.Deleted;
  }

  public Task ClearAsync()
  {
    _ingredients.Clear();
    return Task.CompletedTask;
  }
}
