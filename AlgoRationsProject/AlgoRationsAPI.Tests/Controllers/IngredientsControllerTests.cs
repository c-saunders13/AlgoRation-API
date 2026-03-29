using AlgoRationsAPI.Controllers;
using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using AlgoRationsAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
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
  public void GetAll_ReturnsOk_WithMappedDtos()
  {
    var ingredients = new List<Ingredient>
    {
      new() { Id = Guid.NewGuid(), Name = "Meat", AvailableQuantity = 2 },
      new() { Id = Guid.NewGuid(), Name = "Dough", AvailableQuantity = 2 }
    };
    _repository.GetAll().Returns(ingredients);

    var result = _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dtos = Assert.IsAssignableFrom<IEnumerable<IngredientDto>>(ok.Value).ToList();
    Assert.Equal(2, dtos.Count);
    Assert.Equal("Meat", dtos[0].Name);
    Assert.Equal(2, dtos[0].AvailableQuantity);
    Assert.Equal("Dough", dtos[1].Name);
    Assert.Equal(2, dtos[1].AvailableQuantity);
  }

  [Fact]
  public void GetAll_ReturnsEmptyList_WhenNoIngredients()
  {
    _repository.GetAll().Returns([]);

    var result = _controller.GetAll();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    Assert.Empty(Assert.IsAssignableFrom<IEnumerable<IngredientDto>>(ok.Value));
  }

  // --- GetById ---

  [Fact]
  public void GetById_ReturnsOk_WhenIngredientExists()
  {
    var id = Guid.NewGuid();
    var ingredient = new Ingredient { Id = id, Name = "Meat", AvailableQuantity = 2 };
    _repository.GetById(id).Returns(ingredient);

    var result = _controller.GetById(id);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<IngredientDto>(ok.Value);
    Assert.Equal(id, dto.Id);
    Assert.Equal("Meat", dto.Name);
    Assert.Equal(2, dto.AvailableQuantity);
  }

  [Fact]
  public void GetById_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    _repository.GetById(Arg.Any<Guid>()).Returns((Ingredient?)null);

    var result = _controller.GetById(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result.Result);
  }

  // --- Create ---

  [Fact]
  public void Create_ReturnsCreatedAtAction_WithDto()
  {
    var request = new CreateIngredientRequest("Meat", 2);
    var created = new Ingredient { Id = Guid.NewGuid(), Name = "Meat", AvailableQuantity = 2 };
    _repository.Add(Arg.Any<Ingredient>()).Returns(created);

    var result = _controller.Create(request);

    var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
    Assert.Equal(nameof(_controller.GetById), createdAt.ActionName);
    var dto = Assert.IsType<IngredientDto>(createdAt.Value);
    Assert.Equal(created.Id, dto.Id);
    Assert.Equal("Meat", dto.Name);
    Assert.Equal(2, dto.AvailableQuantity);
  }

  [Fact]
  public void Create_PassesCorrectDataToRepository()
  {
    var request = new CreateIngredientRequest("Meat", 2);
    var created = new Ingredient { Id = Guid.NewGuid(), Name = "Meat", AvailableQuantity = 2 };
    _repository.Add(Arg.Any<Ingredient>()).Returns(created);

    _controller.Create(request);

    _repository.Received(1).Add(Arg.Is<Ingredient>(i =>
      i.Name == "Meat" && i.AvailableQuantity == 2));
  }

  [Fact]
  public void Create_ReturnsValidationProblem_WhenModelStateIsInvalid()
  {
    _controller.ModelState.AddModelError("Name", "Name cannot be empty or whitespace.");

    var result = _controller.Create(new CreateIngredientRequest("", -1));

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
    _repository.DidNotReceive().Add(Arg.Any<Ingredient>());
  }

  // --- Update ---

  [Fact]
  public void Update_ReturnsOk_WhenIngredientExists()
  {
    var id = Guid.NewGuid();
    var request = new UpdateIngredientRequest("Meat Updated", 5);
    var updated = new Ingredient { Id = id, Name = "Meat Updated", AvailableQuantity = 5 };
    _repository.Update(Arg.Any<Ingredient>()).Returns(updated);

    var result = _controller.Update(id, request);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var dto = Assert.IsType<IngredientDto>(ok.Value);
    Assert.Equal(id, dto.Id);
    Assert.Equal("Meat Updated", dto.Name);
    Assert.Equal(5, dto.AvailableQuantity);
  }

  [Fact]
  public void Update_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    _repository.Update(Arg.Any<Ingredient>()).Returns((Ingredient?)null);

    var result = _controller.Update(Guid.NewGuid(), new UpdateIngredientRequest("X", 1));

    Assert.IsType<NotFoundResult>(result.Result);
  }

  [Fact]
  public void Update_PassesIdAndDataToRepository()
  {
    var id = Guid.NewGuid();
    var request = new UpdateIngredientRequest("Meat", 2);
    var updated = new Ingredient { Id = id, Name = "Meat", AvailableQuantity = 2 };
    _repository.Update(Arg.Any<Ingredient>()).Returns(updated);

    _controller.Update(id, request);

    _repository.Received(1).Update(Arg.Is<Ingredient>(i =>
      i.Id == id && i.Name == "Meat" && i.AvailableQuantity == 2));
  }

  [Fact]
  public void Update_ReturnsValidationProblem_WhenModelStateIsInvalid()
  {
    _controller.ModelState.AddModelError("AvailableQuantity", "Available quantity cannot be negative.");

    var result = _controller.Update(Guid.NewGuid(), new UpdateIngredientRequest("Meat", -1));

    var validation = Assert.IsType<BadRequestObjectResult>(result.Result);
    var details = Assert.IsType<ValidationProblemDetails>(validation.Value);
    Assert.Equal(400, details.Status);
    _repository.DidNotReceive().Update(Arg.Any<Ingredient>());
  }

  // --- Delete ---

  [Fact]
  public void Delete_ReturnsNoContent_WhenIngredientExists()
  {
    var id = Guid.NewGuid();
    _repository.Delete(id).Returns(IngredientDeleteResult.Deleted);

    var result = _controller.Delete(id);

    Assert.IsType<NoContentResult>(result);
  }

  [Fact]
  public void Delete_ReturnsNotFound_WhenIngredientDoesNotExist()
  {
    _repository.Delete(Arg.Any<Guid>()).Returns(IngredientDeleteResult.NotFound);

    var result = _controller.Delete(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result);
  }

  [Fact]
  public void Delete_ReturnsConflictWithMessage_WhenIngredientIsIncludedInARecipe()
  {
    var recipeRepository = new RecipeRepository();
    var ingredientRepository = new IngredientRepository(recipeRepository);
    var controller = new IngredientsController(ingredientRepository);
    var ingredient = ingredientRepository.Add(new Ingredient { Name = "Meat", AvailableQuantity = 2 });

    recipeRepository.Add(new Recipe
    {
      Name = "Burger",
      Servings = 1,
      Ingredients =
      [
        new RecipeIngredient { IngredientId = ingredient.Id, RequiredQuantity = 1 }
      ]
    });

    var result = controller.Delete(ingredient.Id);

    var conflict = Assert.IsType<ConflictObjectResult>(result);
    var error = Assert.IsType<ErrorResponse>(conflict.Value);
    Assert.Equal("Ingredient is included in a recipe and cannot be deleted.", error.Message);
  }
}
