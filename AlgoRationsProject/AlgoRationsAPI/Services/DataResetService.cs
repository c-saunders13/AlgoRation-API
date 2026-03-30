using AlgoRationsAPI.Data;
using AlgoRationsAPI.Interfaces;

namespace AlgoRationsAPI.Services;

public class DataResetService(IIngredientRepository ingredientRepository, IRecipeRepository recipeRepository) : IDataResetService
{
  public async Task ResetAsync()
  {
    await recipeRepository.ClearAsync();
    await ingredientRepository.ClearAsync();
    await DataSeeder.SeedAsync(ingredientRepository, recipeRepository);
  }
}
