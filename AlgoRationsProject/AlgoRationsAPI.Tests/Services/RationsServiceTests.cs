using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Services;
using NSubstitute;

namespace AlgoRationsAPI.Tests.Services;

public class RationsServiceTests
{
  private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();
  private readonly IRecipeRepository _recipeRepository = Substitute.For<IRecipeRepository>();

  private RationsService CreateService() => new(_ingredientRepository, _recipeRepository);

  [Fact]
  public void CalculateMaxPeopleFed_ReturnsZero_WhenNoRecipes()
  {
    var cucumberId = Guid.NewGuid();
    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = cucumberId, Name = "Cucumber", AvailableQuantity = 2 }
    ]);
    _recipeRepository.GetAll().Returns([]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(0, result.TotalPeopleFed);
    Assert.Empty(result.Breakdown);
  }

  [Fact]
  public void CalculateMaxPeopleFed_SkipsRecipe_WhenIngredientMissing()
  {
    var meatId = Guid.NewGuid();
    var doughId = Guid.NewGuid();

    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = doughId, Name = "Dough", AvailableQuantity = 10 }
    ]);

    _recipeRepository.GetAll().Returns(
    [
      new Recipe
      {
        Id = Guid.NewGuid(),
        Name = "Pie",
        Servings = 8,
        Ingredients =
        [
          new RecipeIngredient { IngredientId = doughId, RequiredQuantity = 2 },
          new RecipeIngredient { IngredientId = meatId, RequiredQuantity = 2 }
        ]
      }
    ]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(0, result.TotalPeopleFed);
    Assert.Empty(result.Breakdown);
  }

  [Fact]
  public void CalculateMaxPeopleFed_SkipsRecipe_WhenRecipeHasNoIngredients()
  {
    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = Guid.NewGuid(), Name = "Cucumber", AvailableQuantity = 2 }
    ]);

    _recipeRepository.GetAll().Returns(
    [
      new Recipe
      {
        Id = Guid.NewGuid(),
        Name = "Empty Recipe",
        Servings = 4,
        Ingredients = []
      }
    ]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(0, result.TotalPeopleFed);
    Assert.Empty(result.Breakdown);
  }

  [Fact]
  public void CalculateMaxPeopleFed_SkipsRecipe_WhenRequiredQuantityIsZero()
  {
    var cucumberId = Guid.NewGuid();

    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = cucumberId, Name = "Cucumber", AvailableQuantity = 2 }
    ]);

    _recipeRepository.GetAll().Returns(
    [
      new Recipe
      {
        Id = Guid.NewGuid(),
        Name = "Broken Recipe",
        Servings = 4,
        Ingredients =
        [
          new RecipeIngredient { IngredientId = cucumberId, RequiredQuantity = 0 }
        ]
      }
    ]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(0, result.TotalPeopleFed);
    Assert.Empty(result.Breakdown);
  }

  [Fact]
  public void CalculateMaxPeopleFed_SkipsRecipe_WhenRequiredQuantityIsNegative()
  {
    var cucumberId = Guid.NewGuid();

    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = cucumberId, Name = "Cucumber", AvailableQuantity = 2 }
    ]);

    _recipeRepository.GetAll().Returns(
    [
      new Recipe
      {
        Id = Guid.NewGuid(),
        Name = "Broken Recipe",
        Servings = 4,
        Ingredients =
        [
          new RecipeIngredient { IngredientId = cucumberId, RequiredQuantity = -10 }
        ]
      }
    ]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(0, result.TotalPeopleFed);
    Assert.Empty(result.Breakdown);
  }

  [Fact]
  public void CalculateMaxPeopleFed_SortsByServings_AndPrioritizesLargerRecipes()
  {
    var cucumberId = Guid.NewGuid();

    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = cucumberId, Name = "Cucumber", AvailableQuantity = 10 }
    ]);

    var highServingRecipeId = Guid.NewGuid();
    var lowServingRecipeId = Guid.NewGuid();

    _recipeRepository.GetAll().Returns(
    [
      new Recipe
      {
        Id = lowServingRecipeId,
        Name = "Snack",
        Servings = 1,
        Ingredients =
        [
          new RecipeIngredient { IngredientId = cucumberId, RequiredQuantity = 1 }
        ]
      },
      new Recipe
      {
        Id = highServingRecipeId,
        Name = "Family Meal",
        Servings = 5,
        Ingredients =
        [
          new RecipeIngredient { IngredientId = cucumberId, RequiredQuantity = 10 }
        ]
      }
    ]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(5, result.TotalPeopleFed);
    Assert.Single(result.Breakdown);

    var entry = result.Breakdown[0];
    Assert.Equal(highServingRecipeId, entry.RecipeId);
    Assert.Equal("Family Meal", entry.RecipeName);
    Assert.Equal(1, entry.ServingsMade);
    Assert.Equal(5, entry.PeopleFed);
  }

  [Fact]
  public void CalculateMaxPeopleFed_ReturnsBreakdown_ForMultipleRecipes()
  {
    var meatId = Guid.NewGuid();
    var doughId = Guid.NewGuid();

    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = meatId, Name = "Meat", AvailableQuantity = 10 },
      new Ingredient { Id = doughId, Name = "Dough", AvailableQuantity = 4 }
    ]);

    var steakId = Guid.NewGuid();
    var breadId = Guid.NewGuid();

    _recipeRepository.GetAll().Returns(
    [
      new Recipe
      {
        Id = steakId,
        Name = "Steak",
        Servings = 2,
        Ingredients =
        [
          new RecipeIngredient { IngredientId = meatId, RequiredQuantity = 1 }
        ]
      },
      new Recipe
      {
        Id = breadId,
        Name = "Bread",
        Servings = 1,
        Ingredients =
        [
          new RecipeIngredient { IngredientId = doughId, RequiredQuantity = 2 }
        ]
      }
    ]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(22, result.TotalPeopleFed);
    Assert.Equal(2, result.Breakdown.Count);

    var steak = Assert.Single(result.Breakdown, b => b.RecipeId == steakId);
    Assert.Equal(10, steak.ServingsMade);
    Assert.Equal(20, steak.PeopleFed);

    var bread = Assert.Single(result.Breakdown, b => b.RecipeId == breadId);
    Assert.Equal(2, bread.ServingsMade);
    Assert.Equal(2, bread.PeopleFed);
  }

  [Fact]
  public void CalculateMaxPeopleFed_UsesDeterministicTieBreak_WhenServingsAreEqual()
  {
    var ingredientId = Guid.NewGuid();
    _ingredientRepository.GetAll().Returns(
    [
      new Ingredient { Id = ingredientId, Name = "Cucumber", AvailableQuantity = 10 }
    ]);

    var firstId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    var secondId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    _recipeRepository.GetAll().Returns(
    [
      new Recipe
      {
        Id = secondId,
        Name = "Second",
        Servings = 3,
        Ingredients = [new RecipeIngredient { IngredientId = ingredientId, RequiredQuantity = 10 }]
      },
      new Recipe
      {
        Id = firstId,
        Name = "First",
        Servings = 3,
        Ingredients = [new RecipeIngredient { IngredientId = ingredientId, RequiredQuantity = 10 }]
      }
    ]);

    var result = CreateService().CalculateMaxPeopleFed();

    Assert.Equal(3, result.TotalPeopleFed);
    var single = Assert.Single(result.Breakdown);
    Assert.Equal(firstId, single.RecipeId);
    Assert.Equal("First", single.RecipeName);
  }
}
