using AlgoRationsAPI.Controllers;
using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AlgoRationsAPI.Tests.Controllers;

public class RecipesControllerTests
{
  private readonly IRecipeRepository _repository = Substitute.For<IRecipeRepository>();
  private readonly IDataResetService _dataResetService = Substitute.For<IDataResetService>();
  private readonly RecipesController _controller;

  private static readonly Guid IngredientId1 = Guid.NewGuid();
  private static readonly Guid IngredientId2 = Guid.NewGuid();

  public RecipesControllerTests()
  {
    _controller = new RecipesController(_repository, _dataResetService);
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
  public void GetAll_ReturnsOk_WithMappedDtos()
  {
    _repository.GetAll().Returns([MakeRecipe(), MakeRecipe()]);

    var result = _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dtos = Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(ok.Value).ToList();
    Assert.Equal(2, dtos.Count);
  }

  [Fact]
  public void GetAll_MapsDtoFieldsCorrectly()
  {
    var recipe = MakeRecipe();
    _repository.GetAll().Returns([recipe]);

    var result = _controller.GetAll();

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
  public void GetAll_ReturnsEmptyList_WhenNoRecipes()
  {
    _repository.GetAll().Returns([]);

    var result = _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    Assert.Empty(Assert.IsAssignableFrom<IEnumerable<RecipeDto>>(ok.Value));
  }

  // --- GetById ---

  [Fact]
  public void GetById_ReturnsOk_WhenRecipeExists()
  {
    var id = Guid.NewGuid();
    var recipe = MakeRecipe(id);
    _repository.GetById(id).Returns(recipe);

    var result = _controller.GetById(id);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<RecipeDto>(ok.Value);
    Assert.Equal(id, dto.Id);
    Assert.Equal("Pie", dto.Name);
  }

  [Fact]
  public void GetById_ReturnsNotFound_WhenRecipeDoesNotExist()
  {
    _repository.GetById(Arg.Any<Guid>()).Returns((Recipe?)null);

    var result = _controller.GetById(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result.Result);
  }

  // --- Create ---

  [Fact]
  public void Create_ReturnsCreatedAtAction_WithDto()
  {
    var request = MakeCreateRequest();
    Recipe? capturedRecipe = null;
    _repository
      .When(r => r.Add(Arg.Any<Recipe>()))
      .Do(c =>
      {
        capturedRecipe = c.Arg<Recipe>();
        capturedRecipe.Id = Guid.NewGuid();
      });

    var result = _controller.Create(request);

    var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
    Assert.Equal(nameof(_controller.GetById), createdAt.ActionName);
    var dto = Assert.IsType<RecipeDto>(createdAt.Value);
    Assert.Equal("Pie", dto.Name);
    Assert.Equal(4, dto.Servings);
  }

  [Fact]
  public void Create_PassesCorrectDataToRepository()
  {
    var request = MakeCreateRequest();
    _repository.When(r => r.Add(Arg.Any<Recipe>())).Do(_ => { });

    _controller.Create(request);

    _repository.Received(1).Add(Arg.Is<Recipe>(r =>
      r.Name == "Pie" &&
      r.Servings == 4 &&
      r.Ingredients.Count == 2 &&
      r.Ingredients[0].IngredientId == IngredientId1 &&
      r.Ingredients[0].RequiredQuantity == 2));
  }

  [Fact]
  public void Create_MapsIngredientsDtoCorrectly()
  {
    var request = MakeCreateRequest();
    Recipe? capturedRecipe = null;
    _repository
      .When(r => r.Add(Arg.Any<Recipe>()))
      .Do(c => capturedRecipe = c.Arg<Recipe>());

    _controller.Create(request);

    Assert.NotNull(capturedRecipe);
    Assert.Equal(IngredientId2, capturedRecipe!.Ingredients[1].IngredientId);
    Assert.Equal(2, capturedRecipe.Ingredients[1].RequiredQuantity);
  }

  // --- Update ---

  [Fact]
  public void Update_ReturnsOk_WhenRecipeExists()
  {
    var id = Guid.NewGuid();
    var updated = MakeRecipe(id);
    updated.Name = "Updated Pancakes";
    _repository.Update(Arg.Any<Recipe>()).Returns(updated);

    var request = new UpdateRecipeRequest("Updated Pancakes", 4,
      [new RecipeIngredientDto(IngredientId1, 200)]);

    var result = _controller.Update(id, request);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<RecipeDto>(ok.Value);
    Assert.Equal("Updated Pancakes", dto.Name);
  }

  [Fact]
  public void Update_ReturnsNotFound_WhenRecipeDoesNotExist()
  {
    _repository.Update(Arg.Any<Recipe>()).Returns((Recipe?)null);

    var request = new UpdateRecipeRequest("X", 1, []);
    var result = _controller.Update(Guid.NewGuid(), request);

    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public void Update_PassesIdAndDataToRepository()
  {
    var id = Guid.NewGuid();
    var updated = MakeRecipe(id);
    _repository.Update(Arg.Any<Recipe>()).Returns(updated);

    var request = new UpdateRecipeRequest("Pancakes", 4,
      [new RecipeIngredientDto(IngredientId1, 200)]);

    _controller.Update(id, request);

    _repository.Received(1).Update(Arg.Is<Recipe>(r =>
      r.Id == id && r.Name == "Pancakes" && r.Servings == 4));
  }

  // --- Delete ---

  [Fact]
  public void Delete_ReturnsNoContent_WhenRecipeExists()
  {
    var id = Guid.NewGuid();
    _repository.Delete(id).Returns(true);

    var result = _controller.Delete(id);

    Assert.IsType<NoContentResult>(result);
  }

  [Fact]
  public void Delete_ReturnsNotFound_WhenRecipeDoesNotExist()
  {
    _repository.Delete(Arg.Any<Guid>()).Returns(false);

    var result = _controller.Delete(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result);
  }
}
