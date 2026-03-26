using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Repositories;

public class RecipeRepository : IRecipeRepository
{
  private readonly List<Recipe> _recipes = [];

  public IEnumerable<Recipe> GetAll() => _recipes;

  public Recipe? GetById(Guid id) => _recipes.FirstOrDefault(r => r.Id == id);

  public void Add(Recipe recipe)
  {
    recipe.Id = Guid.NewGuid();
    _recipes.Add(recipe);
  }

  public Recipe? Update(Recipe recipe)
  {
    var existing = GetById(recipe.Id);
    if (existing != null)
    {
      existing.Name = recipe.Name;
      existing.Ingredients = recipe.Ingredients;
    }
    return existing;
  }

  public Recipe? Delete(Guid id)
  {
    var recipe = GetById(id);
    if (recipe != null)
    {
      _recipes.Remove(recipe);
    }
    return recipe;
  }
}
