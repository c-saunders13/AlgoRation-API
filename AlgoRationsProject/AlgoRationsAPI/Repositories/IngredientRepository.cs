using AlgoRationsAPI.Data;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoRationsAPI.Repositories;

public class IngredientRepository(AlgoRationsDbContext context, IRecipeRepository recipeRepository) : IIngredientRepository
{
  public async Task<IEnumerable<Ingredient>> GetAllAsync() =>
    await context.Ingredients
      .AsNoTracking()
      .ToListAsync();

  public async Task<Ingredient?> GetByIdAsync(Guid id) =>
    await context.Ingredients.FirstOrDefaultAsync(ingredient => ingredient.Id == id);

  public async Task<Ingredient> AddAsync(Ingredient ingredient)
  {
    ingredient.Id = Guid.NewGuid();
    context.Ingredients.Add(ingredient);
    await context.SaveChangesAsync();
    return ingredient;
  }

  public async Task<Ingredient?> UpdateAsync(Ingredient ingredient)
  {
    var existing = await GetByIdAsync(ingredient.Id);
    if (existing != null)
    {
      existing.Name = ingredient.Name;
      existing.AvailableQuantity = ingredient.AvailableQuantity;
      await context.SaveChangesAsync();
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

    context.Ingredients.Remove(ingredient);
    await context.SaveChangesAsync();
    return IngredientDeleteResult.Deleted;
  }

  public async Task ClearAsync()
  {
    context.Ingredients.RemoveRange(context.Ingredients);
    await context.SaveChangesAsync();
  }
}
