using AlgoRationsAPI.Data;
using AlgoRationsAPI.Interfaces;

namespace AlgoRationsAPI.Services;

public class DataResetService(IIngredientRepository ingredientRepository, IRecipeRepository recipeRepository) : IDataResetService
{
  public void Reset()
  {
    recipeRepository.Clear();
    ingredientRepository.Clear();
    DataSeeder.Seed(ingredientRepository, recipeRepository);
  }
}
