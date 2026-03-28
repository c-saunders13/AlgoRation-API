using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Repositories;

public class IngredientRepository(IRecipeRepository recipeRepository) : IIngredientRepository
{
  private readonly List<Ingredient> _ingredients = [];

  public IEnumerable<Ingredient> GetAll() => _ingredients;

  public Ingredient? GetById(Guid id) => _ingredients.FirstOrDefault(i => i.Id == id);

  public Ingredient Add(Ingredient ingredient)
  {
    ingredient.Id = Guid.NewGuid();
    _ingredients.Add(ingredient);
    return ingredient;
  }

  public Ingredient? Update(Ingredient ingredient)
  {
    var existing = GetById(ingredient.Id);
    if (existing != null)
    {
      existing.Name = ingredient.Name;
      existing.AvailableQuantity = ingredient.AvailableQuantity;
    }
    return existing;
  }

  public IngredientDeleteResult Delete(Guid id)
  {
    var ingredient = GetById(id);
    if (ingredient == null)
    {
      return IngredientDeleteResult.NotFound;
    }

    if (recipeRepository.IsIngredientInUse(id))
    {
      return IngredientDeleteResult.InUse;
    }

    _ingredients.Remove(ingredient);
    return IngredientDeleteResult.Deleted;
  }

  public void Clear() => _ingredients.Clear();
}
