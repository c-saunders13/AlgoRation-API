using AlgoRationsAPI.Models;
using AlgoRationsAPI.Repositories;

namespace AlgoRationsAPI.Tests.Repositories;

public class IngredientRepositoryTests
{
  [Fact]
  public async Task Update_PersistsAllIngredientProperties()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    var original = await repository.AddAsync(new Ingredient
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

    var result = await repository.UpdateAsync(update);

    Assert.NotNull(result);
    Assert.Equal(update.Id, result!.Id);
    Assert.Equal("Updated", result.Name);
    Assert.Equal(6, result.AvailableQuantity);

    var persisted = await repository.GetByIdAsync(update.Id);
    Assert.NotNull(persisted);
    Assert.Equal("Updated", persisted!.Name);
    Assert.Equal(6, persisted.AvailableQuantity);
  }

  [Fact]
  public async Task Delete_ReturnsInUse_WhenIngredientBelongsToARecipe()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    var ingredient = await repository.AddAsync(new Ingredient
    {
      Name = "Meat",
      AvailableQuantity = 2
    });

    await recipeRepository.AddAsync(new Recipe
    {
      Name = "Burger",
      Servings = 1,
      Ingredients =
      [
        new RecipeIngredient { IngredientId = ingredient.Id, RequiredQuantity = 1 }
      ]
    });

    var result = await repository.DeleteAsync(ingredient.Id);

    Assert.Equal(IngredientDeleteResult.InUse, result);
    Assert.NotNull(await repository.GetByIdAsync(ingredient.Id));
  }

  [Fact]
  public async Task Delete_ReturnsDeleted_WhenIngredientExistsAndIsNotUsed()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    var ingredient = await repository.AddAsync(new Ingredient
    {
      Name = "Cheese",
      AvailableQuantity = 4
    });

    var result = await repository.DeleteAsync(ingredient.Id);

    Assert.Equal(IngredientDeleteResult.Deleted, result);
    Assert.Null(await repository.GetByIdAsync(ingredient.Id));
  }

  [Fact]
  public async Task Delete_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);

    var result = await repository.DeleteAsync(Guid.NewGuid());

    Assert.Equal(IngredientDeleteResult.NotFound, result);
  }

  [Fact]
  public async Task Clear_RemovesAllIngredients()
  {
    var recipeRepository = new RecipeRepository();
    var repository = new IngredientRepository(recipeRepository);
    await repository.AddAsync(new Ingredient { Name = "One", AvailableQuantity = 1 });
    await repository.AddAsync(new Ingredient { Name = "Two", AvailableQuantity = 2 });

    await repository.ClearAsync();

    Assert.Empty(await repository.GetAllAsync());
  }
}
