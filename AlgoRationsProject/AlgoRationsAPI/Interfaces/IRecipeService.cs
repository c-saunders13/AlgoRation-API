using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Services;

namespace AlgoRationsAPI.Interfaces;

public interface IRecipeService
{
  Task<IEnumerable<Recipe>> GetAllAsync();
  Task<Recipe?> GetByIdAsync(Guid id);
  Task<RecipeMutationResult> CreateAsync(CreateRecipeRequest request);
  Task<RecipeMutationResult> UpdateAsync(Guid id, UpdateRecipeRequest request);
  Task<bool> DeleteAsync(Guid id);
}