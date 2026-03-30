using AlgoRationsAPI.Data;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AlgoRationsAPI.Tests.Repositories;

public class RecipeRepositoryTests
{
  [Fact]
  public async Task Update_PersistsAllRecipeProperties()
  {
    var dbName = Guid.NewGuid().ToString();
    var options = new DbContextOptionsBuilder<AlgoRationsDbContext>()
      .UseInMemoryDatabase(dbName)
      .Options;

    var context = new AlgoRationsDbContext(options);
    var repository = new RecipeRepository(context);
    var original = new Recipe
    {
      Name = "Original",
      Servings = 1,
      Ingredients =
      [
        new RecipeIngredient { IngredientId = Guid.NewGuid(), RequiredQuantity = 1 }
      ]
    };

    await repository.AddAsync(original);

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

    var result = await repository.UpdateAsync(update);

    Assert.NotNull(result);
    Assert.Equal(update.Id, result!.Id);
    Assert.Equal("Updated", result.Name);
    Assert.Equal(4, result.Servings);
    var ingredient = Assert.Single(result.Ingredients);
    Assert.Equal(updatedIngredientId, ingredient.IngredientId);
    Assert.Equal(3, ingredient.RequiredQuantity);

    // Verify persistence with a separate DbContext
    var verifyOptions = new DbContextOptionsBuilder<AlgoRationsDbContext>()
      .UseInMemoryDatabase(dbName)
      .Options;

    var verifyContext = new AlgoRationsDbContext(verifyOptions);
    var verifyRepository = new RecipeRepository(verifyContext);
    var persisted = await verifyRepository.GetByIdAsync(update.Id);
    Assert.NotNull(persisted);
    Assert.Equal("Updated", persisted!.Name);
    Assert.Equal(4, persisted.Servings);
    var persistedIngredient = Assert.Single(persisted.Ingredients);
    Assert.Equal(updatedIngredientId, persistedIngredient.IngredientId);
    Assert.Equal(3, persistedIngredient.RequiredQuantity);
  }
}
