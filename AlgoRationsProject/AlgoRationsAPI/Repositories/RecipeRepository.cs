using AlgoRationsAPI.Data;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoRationsAPI.Repositories;

public class RecipeRepository(AlgoRationsDbContext context) : IRecipeRepository
{
  public async Task<IEnumerable<Recipe>> GetAllAsync() =>
    await context.Recipes
      .AsNoTracking()
      .Include(recipe => recipe.Ingredients)
      .ToListAsync();

  public async Task<Recipe?> GetByIdAsync(Guid id) =>
    await context.Recipes
      .Include(recipe => recipe.Ingredients)
      .FirstOrDefaultAsync(recipe => recipe.Id == id);

  public Task AddAsync(Recipe recipe)
  {
    recipe.Id = Guid.NewGuid();
    context.Recipes.Add(recipe);
    return context.SaveChangesAsync();
  }

  public async Task<Recipe?> UpdateAsync(Recipe recipe)
  {
    var existing = await context.Recipes
      .Include(existingRecipe => existingRecipe.Ingredients)
      .FirstOrDefaultAsync(existingRecipe => existingRecipe.Id == recipe.Id);

    if (existing != null)
    {
      existing.Name = recipe.Name;
      existing.Servings = recipe.Servings;

      existing.Ingredients.Clear();
      foreach (var ingredient in recipe.Ingredients)
      {
        existing.Ingredients.Add(new RecipeIngredient
        {
          IngredientId = ingredient.IngredientId,
          RequiredQuantity = ingredient.RequiredQuantity
        });
      }

      await context.SaveChangesAsync();
    }

    return existing;
  }

  public async Task<bool> DeleteAsync(Guid id)
  {
    var recipe = await context.Recipes
      .Include(existingRecipe => existingRecipe.Ingredients)
      .FirstOrDefaultAsync(existingRecipe => existingRecipe.Id == id);

    if (recipe != null)
    {
      context.Recipes.Remove(recipe);
      await context.SaveChangesAsync();
      return true;
    }

    return false;
  }

  public async Task<bool> IsIngredientInUseAsync(Guid ingredientId) =>
    await context.Recipes.AnyAsync(recipe => recipe.Ingredients.Any(ingredient => ingredient.IngredientId == ingredientId));

  public async Task ClearAsync()
  {
    context.Recipes.RemoveRange(context.Recipes);
    await context.SaveChangesAsync();
  }
}
