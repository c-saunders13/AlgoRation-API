using AlgoRationsAPI.Controllers;
using AlgoRationsAPI.Data;
using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace AlgoRationsAPI.Tests.Controllers;

public class IngredientsControllerTests
{
  private readonly IIngredientRepository _repository = Substitute.For<IIngredientRepository>();
  private readonly IngredientsController _controller;

  public IngredientsControllerTests()
  {
    _controller = new IngredientsController(_repository);
  }

  // --- GetAll ---

  [Fact]
  public async Task GetAll_ReturnsOk_WithMappedDtos()
  {
    var ingredients = new List<Ingredient>
    {
      new() { Id = Guid.NewGuid(), Name = "Meat", AvailableQuantity = 2 },
      new() { Id = Guid.NewGuid(), Name = "Dough", AvailableQuantity = 2 }
    };
    _repository.GetAllAsync().Returns(Task.FromResult<IEnumerable<Ingredient>>(ingredients));

    var result = await _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dtos = Assert.IsAssignableFrom<IEnumerable<IngredientDto>>(ok.Value).ToList();
    Assert.Equal(2, dtos.Count);
    Assert.Equal("Meat", dtos[0].Name);
    Assert.Equal(2, dtos[0].AvailableQuantity);
    Assert.Equal("Dough", dtos[1].Name);
    Assert.Equal(2, dtos[1].AvailableQuantity);
  }

  [Fact]
  public async Task GetAll_ReturnsEmptyList_WhenNoIngredients()
  {
    _repository.GetAllAsync().Returns(Task.FromResult<IEnumerable<Ingredient>>([]));

    var result = await _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    Assert.Empty(Assert.IsAssignableFrom<IEnumerable<IngredientDto>>(ok.Value));
  }

  // --- GetById ---

  [Fact]
  public async Task GetById_ReturnsOk_WhenIngredientExists()
  {
    var id = Guid.NewGuid();
    var ingredient = new Ingredient { Id = id, Name = "Meat", AvailableQuantity = 2 };
    _repository.GetByIdAsync(id).Returns(Task.FromResult<Ingredient?>(ingredient));

    var result = await _controller.GetById(id);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<IngredientDto>(ok.Value);
    Assert.Equal(id, dto.Id);
    Assert.Equal("Meat", dto.Name);
    Assert.Equal(2, dto.AvailableQuantity);
  }

  [Fact]
  public async Task GetById_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    _repository.GetByIdAsync(Arg.Any<Guid>()).Returns(Task.FromResult<Ingredient?>(null));

    var result = await _controller.GetById(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result.Result);
  }

  // --- Create ---

  [Fact]
  public async Task Create_ReturnsCreatedAtAction_WithDto()
  {
    var request = new CreateIngredientRequest("Meat", 2);
    var created = new Ingredient { Id = Guid.NewGuid(), Name = "Meat", AvailableQuantity = 2 };
    _repository.AddAsync(Arg.Any<Ingredient>()).Returns(Task.FromResult(created));

    var result = await _controller.Create(request);

    var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
    Assert.Equal(nameof(_controller.GetById), createdAt.ActionName);
    var dto = Assert.IsType<IngredientDto>(createdAt.Value);
    Assert.Equal(created.Id, dto.Id);
    Assert.Equal("Meat", dto.Name);
    Assert.Equal(2, dto.AvailableQuantity);
  }

  [Fact]
  public async Task Create_PassesCorrectDataToRepository()
  {
    var request = new CreateIngredientRequest("Meat", 2);
    var created = new Ingredient { Id = Guid.NewGuid(), Name = "Meat", AvailableQuantity = 2 };
    _repository.AddAsync(Arg.Any<Ingredient>()).Returns(Task.FromResult(created));

    await _controller.Create(request);

    await _repository.Received(1).AddAsync(Arg.Is<Ingredient>(i =>
      i.Name == "Meat" && i.AvailableQuantity == 2));
  }

  [Fact]
  public async Task Create_ReturnsValidationProblem_WhenModelStateIsInvalid()
  {
    _controller.ModelState.AddModelError("Name", "Name cannot be empty or whitespace.");

    var result = await _controller.Create(new CreateIngredientRequest("", -1));

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
    await _repository.DidNotReceive().AddAsync(Arg.Any<Ingredient>());
  }

  // --- Update ---

  [Fact]
  public async Task Update_ReturnsOk_WhenIngredientExists()
  {
    var id = Guid.NewGuid();
    var request = new UpdateIngredientRequest("Meat Updated", 5);
    var updated = new Ingredient { Id = id, Name = "Meat Updated", AvailableQuantity = 5 };
    _repository.UpdateAsync(Arg.Any<Ingredient>()).Returns(Task.FromResult<Ingredient?>(updated));

    var result = await _controller.Update(id, request);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<IngredientDto>(ok.Value);
    Assert.Equal(id, dto.Id);
    Assert.Equal("Meat Updated", dto.Name);
    Assert.Equal(5, dto.AvailableQuantity);
  }

  [Fact]
  public async Task Update_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    _repository.UpdateAsync(Arg.Any<Ingredient>()).Returns(Task.FromResult<Ingredient?>(null));

    var result = await _controller.Update(Guid.NewGuid(), new UpdateIngredientRequest("X", 1));

    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public async Task Update_PassesIdAndDataToRepository()
  {
    var id = Guid.NewGuid();
    var request = new UpdateIngredientRequest("Meat", 2);
    var updated = new Ingredient { Id = id, Name = "Meat", AvailableQuantity = 2 };
    _repository.UpdateAsync(Arg.Any<Ingredient>()).Returns(Task.FromResult<Ingredient?>(updated));

    await _controller.Update(id, request);

    await _repository.Received(1).UpdateAsync(Arg.Is<Ingredient>(i =>
      i.Id == id && i.Name == "Meat" && i.AvailableQuantity == 2));
  }

  [Fact]
  public async Task Update_ReturnsValidationProblem_WhenModelStateIsInvalid()
  {
    _controller.ModelState.AddModelError("AvailableQuantity", "Available quantity cannot be negative.");

    var result = await _controller.Update(Guid.NewGuid(), new UpdateIngredientRequest("Meat", -1));

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
    await _repository.DidNotReceive().UpdateAsync(Arg.Any<Ingredient>());
  }

  // --- Delete ---

  [Fact]
  public async Task Delete_ReturnsNoContent_WhenIngredientExists()
  {
    var id = Guid.NewGuid();
    _repository.DeleteAsync(id).Returns(Task.FromResult(IngredientDeleteResult.Deleted));

    var result = await _controller.Delete(id);

    Assert.IsType<NoContentResult>(result);
  }

  [Fact]
  public async Task Delete_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    _repository.DeleteAsync(Arg.Any<Guid>()).Returns(Task.FromResult(IngredientDeleteResult.NotFound));

    var result = await _controller.Delete(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public async Task Delete_ReturnsConflictWithMessage_WhenIngredientIsIncludedInARecipe()
  {
    var options = new DbContextOptionsBuilder<AlgoRationsDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options;

    var context = new AlgoRationsDbContext(options);
    var recipeRepository = new RecipeRepository(context);
    var ingredientRepository = new IngredientRepository(context, recipeRepository);
    var controller = new IngredientsController(ingredientRepository);
    var ingredient = await ingredientRepository.AddAsync(new Ingredient { Name = "Meat", AvailableQuantity = 2 });

    await recipeRepository.AddAsync(new Recipe
    {
      Name = "Burger",
      Servings = 1,
      Ingredients =
      [
        new RecipeIngredient { IngredientId = ingredient.Id, RequiredQuantity = 1 }
      ]
    });

    var result = await controller.Delete(ingredient.Id);

    var conflict = Assert.IsType<ConflictObjectResult>(result);
    var error = Assert.IsType<ErrorResponse>(conflict.Value);
    Assert.Equal("Ingredient is included in a recipe and cannot be deleted.", error.Message);
  }
}
