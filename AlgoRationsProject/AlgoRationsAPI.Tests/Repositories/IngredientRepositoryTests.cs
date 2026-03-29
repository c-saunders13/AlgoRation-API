using AlgoRationsAPI.Models;
using AlgoRationsAPI.Repositories;

namespace AlgoRationsAPI.Tests.Repositories;

public class IngredientRepositoryTests
{
  [Fact]
  public void Update_PersistsAllIngredientProperties()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    var original = repository.Add(new Ingredient
    {
      Name = "Original",
      AvailableQuantity = 1
    });

    var update = new Ingredient
    {
      Id = original.Id,
      Name = "Updated",
      AvailableQuantity = 6
    };

    var result = repository.Update(update);

    Assert.NotNull(result);
    Assert.Equal(update.Id, result!.Id);
    Assert.Equal("Updated", result.Name);
    Assert.Equal(6, result.AvailableQuantity);

    var persisted = repository.GetById(update.Id);
    Assert.NotNull(persisted);
    Assert.Equal("Updated", persisted!.Name);
    Assert.Equal(6, persisted.AvailableQuantity);
  }

  [Fact]
  public void Delete_ReturnsInUse_WhenIngredientBelongsToARecipe()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    var ingredient = repository.Add(new Ingredient
    {
      Name = "Meat",
      AvailableQuantity = 2
    });

    recipeRepository.Add(new Recipe
    {
      Name = "Burger",
      Servings = 1,
      Ingredients =
      [
        new RecipeIngredient { IngredientId = ingredient.Id, RequiredQuantity = 1 }
      ]
    });

    var result = repository.Delete(ingredient.Id);

    Assert.Equal(IngredientDeleteResult.InUse, result);
    Assert.NotNull(repository.GetById(ingredient.Id));
  }

  [Fact]
  public void Delete_ReturnsDeleted_WhenIngredientExistsAndIsNotUsed()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    var ingredient = repository.Add(new Ingredient
    {
      Name = "Cheese",
      AvailableQuantity = 4
    });

    var result = repository.Delete(ingredient.Id);

    Assert.Equal(IngredientDeleteResult.Deleted, result);
    Assert.Null(repository.GetById(ingredient.Id));
  }

  [Fact]
  public void Delete_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);

    var result = repository.Delete(Guid.NewGuid());

    Assert.Equal(IngredientDeleteResult.NotFound, result);
  }

  [Fact]
  public void Clear_RemovesAllIngredients()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    repository.Add(new Ingredient { Name = "One", AvailableQuantity = 1 });
    repository.Add(new Ingredient { Name = "Two", AvailableQuantity = 2 });

    repository.Clear();

    Assert.Empty(repository.GetAll());
  }
}
