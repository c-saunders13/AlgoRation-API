using AlgoRationsAPI.Data;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AlgoRationsAPI.Tests.Repositories;

public class IngredientRepositoryTests
{
  private static (IngredientRepository ingredientRepository, RecipeRepository recipeRepository) CreateRepositories()
  {
    var options = new DbContextOptionsBuilder<AlgoRationsDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    var context = new AlgoRationsDbContext(options);
    var recipeRepository = new RecipeRepository(context);
    var ingredientRepository = new IngredientRepository(context, recipeRepository);
    return (ingredientRepository, recipeRepository);
  }

  [Fact]
  public async Task Update_PersistsAllIngredientProperties()
  {
    var dbName = Guid.NewGuid().ToString();
    var options = new DbContextOptionsBuilder<AlgoRationsDbContext>()
      .UseInMemoryDatabase(dbName)
      .Options;

    var context = new AlgoRationsDbContext(options);
    var recipeRepository = new RecipeRepository(context);
    var repository = new IngredientRepository(context, recipeRepository);
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

    // Verify persistence with a separate DbContext
    var verifyOptions = new DbContextOptionsBuilder<AlgoRationsDbContext>()
      .UseInMemoryDatabase(dbName)
      .Options;

    var verifyContext = new AlgoRationsDbContext(verifyOptions);
    var verifyRepository = new IngredientRepository(verifyContext, new RecipeRepository(verifyContext));
    var persisted = await verifyRepository.GetByIdAsync(update.Id);
    Assert.NotNull(persisted);
    Assert.Equal("Updated", persisted!.Name);
    Assert.Equal(6, persisted.AvailableQuantity);
  }

  [Fact]
  public async Task Delete_ReturnsInUse_WhenIngredientBelongsToARecipe()
  {
    var (repository, recipeRepository) = CreateRepositories();
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
    var (repository, _) = CreateRepositories();
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
    var (repository, _) = CreateRepositories();

    var result = await repository.DeleteAsync(Guid.NewGuid());

    Assert.Equal(IngredientDeleteResult.NotFound, result);
  }

  [Fact]
  public async Task Clear_RemovesAllIngredients()
  {
    var (repository, _) = CreateRepositories();
    await repository.AddAsync(new Ingredient { Name = "One", AvailableQuantity = 1 });
    await repository.AddAsync(new Ingredient { Name = "Two", AvailableQuantity = 2 });

    await repository.ClearAsync();

    Assert.Empty(await repository.GetAllAsync());
  }
}
