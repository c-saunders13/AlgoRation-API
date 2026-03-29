using AlgoRationsAPI.Models;
using AlgoRationsAPI.Repositories;

namespace AlgoRationsAPI.Tests.Repositories;

public class RecipeRepositoryTests
{
  [Fact]
  public void Update_PersistsAllRecipeProperties()
  {
    var repository = new RecipeRepository();
    var original = new Recipe
    {
      Name = "Original",
      Servings = 1,
      Ingredients =
      [
        new RecipeIngredient { IngredientId = Guid.NewGuid(), RequiredQuantity = 1 }
      ]
    };

    repository.Add(original);

    var updatedIngredientId = Guid.NewGuid();
    var update = new Recipe
    {
      Id = original.Id,
      Name = "Updated",
      Servings = 4,
      Ingredients =
      [
        new RecipeIngredient { IngredientId = updatedIngredientId, RequiredQuantity = 3 }
      ]
    };

    var result = repository.Update(update);

    Assert.NotNull(result);
    Assert.Equal(update.Id, result!.Id);
    Assert.Equal("Updated", result.Name);
    Assert.Equal(4, result.Servings);
    var ingredient = Assert.Single(result.Ingredients);
    Assert.Equal(updatedIngredientId, ingredient.IngredientId);
    Assert.Equal(3, ingredient.RequiredQuantity);

    var persisted = repository.GetById(update.Id);
    Assert.NotNull(persisted);
    Assert.Equal("Updated", persisted!.Name);
    Assert.Equal(4, persisted.Servings);
    var persistedIngredient = Assert.Single(persisted.Ingredients);
    Assert.Equal(updatedIngredientId, persistedIngredient.IngredientId);
    Assert.Equal(3, persistedIngredient.RequiredQuantity);
  }
}
