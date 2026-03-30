using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Services;
using NSubstitute;

namespace AlgoRationsAPI.Tests.Services;

public class RecipeServiceTests
{
  private readonly IRecipeRepository _recipeRepository = Substitute.For<IRecipeRepository>();
  private readonly IIngredientRepository _ingredientRepository = Substitute.For<IIngredientRepository>();

  private RecipeService CreateService() => new(_recipeRepository, _ingredientRepository);

  [Fact]
  public async Task CreateAsync_ReturnsValidationError_WhenIngredientQuantityIsNotPositive()
  {
    var request = new CreateRecipeRequest("Pie", 4,
      [new RecipeIngredientDto(Guid.NewGuid(), 0)]);

    var result = await CreateService().CreateAsync(request);

    Assert.True(result.HasValidationErrors);
    Assert.Contains("Ingredients[0].RequiredQuantity", result.ValidationErrors.Keys);
    await _recipeRepository.DidNotReceive().AddAsync(Arg.Any<Recipe>());
  }

  [Fact]
  public async Task CreateAsync_ReturnsValidationError_WhenRecipeContainsDuplicateIngredients()
  {
    var ingredientId = Guid.NewGuid();
    _ingredientRepository.GetByIdAsync(ingredientId)
      .Returns(Task.FromResult<Ingredient?>(new Ingredient { Id = ingredientId, Name = "Meat", AvailableQuantity = 10 }));

    var request = new CreateRecipeRequest("Pie", 4,
      [new RecipeIngredientDto(ingredientId, 2), new RecipeIngredientDto(ingredientId, 1)]);

    var result = await CreateService().CreateAsync(request);

    Assert.True(result.HasValidationErrors);
    Assert.Contains(nameof(RecipeIngredientDto.IngredientId), result.ValidationErrors.Keys);
    await _recipeRepository.DidNotReceive().AddAsync(Arg.Any<Recipe>());
  }

  [Fact]
  public async Task CreateAsync_ReturnsValidationError_WhenIngredientDoesNotExist()
  {
    var missingIngredientId = Guid.NewGuid();
    _ingredientRepository.GetByIdAsync(missingIngredientId).Returns(Task.FromResult<Ingredient?>(null));

    var request = new CreateRecipeRequest("Pie", 4,
      [new RecipeIngredientDto(missingIngredientId, 2)]);

    var result = await CreateService().CreateAsync(request);

    Assert.True(result.HasValidationErrors);
    Assert.Contains(nameof(RecipeIngredientDto.IngredientId), result.ValidationErrors.Keys);
    await _recipeRepository.DidNotReceive().AddAsync(Arg.Any<Recipe>());
  }

  [Fact]
  public async Task CreateAsync_AddsRecipe_WhenRequestIsValid()
  {
    var ingredientId = Guid.NewGuid();
    _ingredientRepository.GetByIdAsync(ingredientId)
      .Returns(Task.FromResult<Ingredient?>(new Ingredient { Id = ingredientId, Name = "Meat", AvailableQuantity = 10 }));

    Recipe? capturedRecipe = null;
    _recipeRepository.When(repository => repository.AddAsync(Arg.Any<Recipe>()))
      .Do(call =>
      {
        capturedRecipe = call.Arg<Recipe>();
        capturedRecipe.Id = Guid.NewGuid();
      });

    var request = new CreateRecipeRequest("Pie", 4,
      [new RecipeIngredientDto(ingredientId, 2)]);

    var result = await CreateService().CreateAsync(request);

    Assert.False(result.HasValidationErrors);
    Assert.NotNull(result.Recipe);
    Assert.Equal("Pie", result.Recipe!.Name);
    await _recipeRepository.Received(1).AddAsync(Arg.Any<Recipe>());
  }

  [Fact]
  public async Task UpdateAsync_ReturnsNotFound_WhenRecipeDoesNotExist()
  {
    var ingredientId = Guid.NewGuid();
    _ingredientRepository.GetByIdAsync(ingredientId)
      .Returns(Task.FromResult<Ingredient?>(new Ingredient { Id = ingredientId, Name = "Meat", AvailableQuantity = 10 }));
    _recipeRepository.UpdateAsync(Arg.Any<Recipe>()).Returns(Task.FromResult<Recipe?>(null));

    var request = new UpdateRecipeRequest("Updated", 2,
      [new RecipeIngredientDto(ingredientId, 1)]);

    var result = await CreateService().UpdateAsync(Guid.NewGuid(), request);

    Assert.True(result.NotFound);
  }
}
