using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Interfaces;

public interface IRecipeRepository
{
  List<Recipe> GetAll();
  Recipe GetById();
  Recipe Add();
  Recipe Update();
  bool Delete();
}
