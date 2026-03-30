using AlgoRationsAPI.Controllers;
using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AlgoRationsAPI.Tests.Controllers;

public class RecipesControllerTests
{
  private readonly IRecipeService _recipeService = Substitute.For<IRecipeService>();
  private readonly IDataResetService _dataResetService = Substitute.For<IDataResetService>();
  private readonly RecipesController _controller;

  private static readonly Guid IngredientId1 = Guid.NewGuid();
  private static readonly Guid IngredientId2 = Guid.NewGuid();

  public RecipesControllerTests()
  {
    _controller = new RecipesController(_recipeService, _dataResetService);
  }

  private static Recipe MakeRecipe(Guid? id = null) => new()
  {
    Id = id ?? Guid.NewGuid(),
    Name = "Pie",
    Servings = 4,
    Ingredients =
    [
      new() { IngredientId = IngredientId1, RequiredQuantity = 2 },
      new() { IngredientId = IngredientId2, RequiredQuantity = 2 }
    ]
  };

  private static CreateRecipeRequest MakeCreateRequest() => new(
    "Pie",
    4,
    [
      new RecipeIngredientDto(IngredientId1, 2),
      new RecipeIngredientDto(IngredientId2, 2)
    ]
  );

  // --- GetAll ---

  [Fact]
  public async Task GetAll_ReturnsOk_WithMappedDtos()
  {
    _recipeService.GetAllAsync().Returns(Task.FromResult<IEnumerable<Recipe>>([MakeRecipe(), MakeRecipe()]));

    var result = await _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dtos = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(ok.Value).ToList();
    Assert.Equal(2, dtos.Count);
  }

  [Fact]
  public async Task GetAll_MapsDtoFieldsCorrectly()
  {
    var recipe = MakeRecipe();
    _recipeService.GetAllAsync().Returns(Task.FromResult<IEnumerable<Recipe>>([recipe]));

    var result = await _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(ok.Value).Single();
    Assert.Equal(recipe.Id, dto.Id);
    Assert.Equal("Pie", dto.Name);
    Assert.Equal(4, dto.Servings);
    Assert.Equal(2, dto.Ingredients.Count);
    Assert.Equal(IngredientId1, dto.Ingredients[0].IngredientId);
    Assert.Equal(2, dto.Ingredients[0].RequiredQuantity);
  }

  [Fact]
  public async Task GetAll_ReturnsEmptyList_WhenNoRecipes()
  {
    _recipeService.GetAllAsync().Returns(Task.FromResult<IEnumerable<Recipe>>([]));

    var result = await _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    Assert.Empty(Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(ok.Value));
  }

  // --- GetById ---

  [Fact]
  public async Task GetById_ReturnsOk_WhenRecipeExists()
  {
    var id = Guid.NewGuid();
    var recipe = MakeRecipe(id);
    _recipeService.GetByIdAsync(id).Returns(Task.FromResult<Recipe?>(recipe));

    var result = await _controller.GetById(id);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<RecipeDto>(ok.Value);
    Assert.Equal(id, dto.Id);
    Assert.Equal("Pie", dto.Name);
  }

  [Fact]
  public async Task GetById_ReturnsNotFound_WhenRecipeDoesNotExist()
  {
    _recipeService.GetByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Recipe?>(null));

    var result = await _controller.GetById(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result.Result);
  }

  // --- Create ---

  [Fact]
  public async Task Create_ReturnsCreatedAtAction_WithDto()
  {
    var request = MakeCreateRequest();
    var created = MakeRecipe(Guid.NewGuid());
    _recipeService.CreateAsync(request).Returns(Task.FromResult(RecipeMutationResult.Success(created)));

    var result = await _controller.Create(request);

    var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
    Assert.Equal(nameof(_controller.GetById), createdAt.ActionName);
    var dto = Assert.IsType<RecipeDto>(createdAt.Value);
    Assert.Equal("Pie", dto.Name);
    Assert.Equal(4, dto.Servings);
  }

  [Fact]
  public async Task Create_PassesRequestToService()
  {
    var request = MakeCreateRequest();
    _recipeService.CreateAsync(request).Returns(Task.FromResult(RecipeMutationResult.Success(MakeRecipe(Guid.NewGuid()))));

    await _controller.Create(request);

    await _recipeService.Received(1).CreateAsync(Arg.Is<CreateRecipeRequest>(r =>
      r.Name == "Pie" &&
      r.Servings == 4 &&
      r.Ingredients.Count == 2 &&
      r.Ingredients[0].IngredientId == IngredientId1 &&
      r.Ingredients[0].RequiredQuantity == 2));
  }

  [Fact]
  public async Task Create_ReturnsValidationProblem_WhenModelStateIsInvalid()
  {
    _controller.ModelState.AddModelError("Servings", "Servings must be greater than zero.");

    var request = new CreateRecipeRequest("Pie", 0,
      [new RecipeIngredientDto(IngredientId1, 2)]);

    var result = await _controller.Create(request);

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
    await _recipeService.DidNotReceive().CreateAsync(Arg.Any<CreateRecipeRequest>());
  }

  [Fact]
  public async Task Create_ReturnsValidationProblem_WhenServiceValidationFails()
  {
    var request = MakeCreateRequest();
    _recipeService.CreateAsync(request).Returns(Task.FromResult(RecipeMutationResult.ValidationFailed(new Dictionary<string, string[]>
    {
      { nameof(RecipeIngredientDto.IngredientId), ["Ingredient does not exist."] }
    })));

    var result = await _controller.Create(request);

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
  }

  // --- Update ---

  [Fact]
  public async Task Update_ReturnsOk_WhenRecipeExists()
  {
    var id = Guid.NewGuid();
    var updated = MakeRecipe(id);
    updated.Name = "Updated Pancakes";
    _recipeService.UpdateAsync(id, Arg.Any<UpdateRecipeRequest>())
      .Returns(Task.FromResult(RecipeMutationResult.Success(updated)));

    var request = new UpdateRecipeRequest("Updated Pancakes", 4,
      [new RecipeIngredientDto(IngredientId1, 200)]);

    var result = await _controller.Update(id, request);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<RecipeDto>(ok.Value);
    Assert.Equal("Updated Pancakes", dto.Name);
  }

  [Fact]
  public async Task Update_ReturnsNotFound_WhenRecipeDoesNotExist()
  {
    var id = Guid.NewGuid();
    _recipeService.UpdateAsync(id, Arg.Any<UpdateRecipeRequest>())
      .Returns(Task.FromResult(RecipeMutationResult.NotFoundResult()));

    var request = new UpdateRecipeRequest("X", 1, [new RecipeIngredientDto(IngredientId1, 1)]);
    var result = await _controller.Update(id, request);

    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public async Task Update_PassesIdAndRequestToService()
  {
    var id = Guid.NewGuid();
    var updated = MakeRecipe(id);
    _recipeService.UpdateAsync(id, Arg.Any<UpdateRecipeRequest>())
      .Returns(Task.FromResult(RecipeMutationResult.Success(updated)));

    var request = new UpdateRecipeRequest("Pancakes", 4,
      [new RecipeIngredientDto(IngredientId1, 200)]);

    await _controller.Update(id, request);

    await _recipeService.Received(1).UpdateAsync(id, Arg.Is<UpdateRecipeRequest>(r =>
      r.Name == "Pancakes" && r.Servings == 4));
  }

  [Fact]
  public async Task Update_ReturnsValidationProblem_WhenModelStateIsInvalid()
  {
    _controller.ModelState.AddModelError("Ingredients", "At least one ingredient is required.");

    var request = new UpdateRecipeRequest("Pancakes", 4, []);
    var result = await _controller.Update(Guid.NewGuid(), request);

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
    await _recipeService.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<UpdateRecipeRequest>());
  }

  [Fact]
  public async Task Update_ReturnsValidationProblem_WhenServiceValidationFails()
  {
    var id = Guid.NewGuid();
    _recipeService.UpdateAsync(id, Arg.Any<UpdateRecipeRequest>())
      .Returns(Task.FromResult(RecipeMutationResult.ValidationFailed(new Dictionary<string, string[]>
      {
        { nameof(RecipeIngredientDto.IngredientId), ["Ingredient does not exist."] }
      })));

    var request = new UpdateRecipeRequest("Pancakes", 4,
      [new RecipeIngredientDto(IngredientId1, 2)]);

    var result = await _controller.Update(id, request);

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
  }

  // --- Delete ---

  [Fact]
  public async Task Delete_ReturnsNoContent_WhenRecipeExists()
  {
    var id = Guid.NewGuid();
    _recipeService.DeleteAsync(id).Returns(Task.FromResult(true));

    var result = await _controller.Delete(id);

    Assert.IsType<NoContentResult>(result);
  }

  [Fact]
  public async Task Delete_ReturnsNotFound_WhenRecipeDoesNotExist()
  {
    _recipeService.DeleteAsync(Arg.Any<Guid>()).Returns(Task.FromResult(false));

    var result = await _controller.Delete(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result);
  }
}
