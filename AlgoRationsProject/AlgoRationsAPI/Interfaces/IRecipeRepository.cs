using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Interfaces;

public interface IRecipeRepository
{
  Task<IEnumerable<Recipe>> GetAllAsync();
  Task<Recipe?> GetByIdAsync(Guid id);
  Task AddAsync(Recipe recipe);
  Task<Recipe?> UpdateAsync(Recipe recipe);
  Task<bool> DeleteAsync(Guid id);
  Task<bool> IsIngredientInUseAsync(Guid ingredientId);
  Task ClearAsync();
}
