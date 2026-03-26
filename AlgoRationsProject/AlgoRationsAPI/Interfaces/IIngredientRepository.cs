using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Interfaces;

public interface IIngredientRepository
{
  IEnumerable<Ingredient> GetAll();
  Ingredient? GetById(Guid id);
  Ingredient Add(Ingredient ingredient);
  Ingredient? Update(Ingredient ingredient);
  Ingredient? Delete(Guid id);
}
