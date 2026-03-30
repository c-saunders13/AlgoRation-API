using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Interfaces;

public interface IIngredientRepository
{
  Task<IEnumerable<Ingredient>> GetAllAsync();
  Task<Ingredient?> GetByIdAsync(Guid id);
  Task<Ingredient> AddAsync(Ingredient ingredient);
  Task<Ingredient?> UpdateAsync(Ingredient ingredient);
  Task<IngredientDeleteResult> DeleteAsync(Guid id);
  Task ClearAsync();
}
