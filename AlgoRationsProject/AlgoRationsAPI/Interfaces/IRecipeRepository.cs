using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Interfaces;

public interface IRecipeRepository
{
  IEnumerable<Recipe> GetAll();
  Recipe? GetById(Guid id);
  void Add(Recipe recipe);
  Recipe? Update(Recipe recipe);
  bool Delete(Guid id);
  bool IsIngredientInUse(Guid ingredientId);
}
