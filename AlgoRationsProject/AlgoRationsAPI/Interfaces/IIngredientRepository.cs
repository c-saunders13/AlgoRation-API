using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Interfaces;

public interface IIngredientRepository
{
  List<Ingredient> GetAll();
  Ingredient GetById();
  Ingredient Add();
  Ingredient Update();
  bool Delete();
}
