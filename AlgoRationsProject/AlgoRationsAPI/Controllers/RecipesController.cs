using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlgoRationsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController(
  IRecipeRepository repository,
  IIngredientRepository ingredientRepository,
  IDataResetService dataResetService) : ControllerBase
{
  [HttpGet]
  public ActionResult<IEnumerable<RecipeDto>> GetAll() =>
      Ok(repository.GetAll().Select(recipe => new RecipeDto(
        recipe.Id,
        recipe.Name,
        recipe.Servings,
        [.. recipe.Ingredients.Select(i => new RecipeIngredientDto(
          i.IngredientId,
          i.RequiredQuantity
        ))]
      )));

  [HttpGet("{id:guid}")]
  public ActionResult<RecipeDto> GetById(Guid id)
  {
    var recipe = repository.GetById(id);
    if (recipe == null)
    {
      return NotFound();
    }

    return Ok(new RecipeDto(
      recipe.Id,
      recipe.Name,
      recipe.Servings,
      [.. recipe.Ingredients.Select(i => new RecipeIngredientDto(
        i.IngredientId,
        i.RequiredQuantity
      ))]
    ));
  }

  [HttpPost]
  public ActionResult<RecipeDto> Create(CreateRecipeRequest request)
  {
    ValidateRecipeRequest(request.Ingredients);
    if (!ModelState.IsValid)
    {
      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    var recipe = new Recipe
    {
      Name = request.Name,
      Servings = request.Servings,
      Ingredients = [.. request.Ingredients.Select(i => new RecipeIngredient
      {
        IngredientId = i.IngredientId,
        RequiredQuantity = i.RequiredQuantity
      })]
    };

    repository.Add(recipe);
    return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, new RecipeDto(
      recipe.Id,
      recipe.Name,
      recipe.Servings,
      [.. recipe.Ingredients.Select(i => new RecipeIngredientDto(
        i.IngredientId,
        i.RequiredQuantity
      ))]
    ));
  }

  [HttpPut("{id:guid}")]
  public ActionResult<RecipeDto> Update(Guid id, UpdateRecipeRequest request)
  {
    ValidateRecipeRequest(request.Ingredients);
    if (!ModelState.IsValid)
    {
      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    var recipe = new Recipe
    {
      Id = id,
      Name = request.Name,
      Servings = request.Servings,
      Ingredients = [.. request.Ingredients.Select(i => new RecipeIngredient
      {
        IngredientId = i.IngredientId,
        RequiredQuantity = i.RequiredQuantity
      })]
    };

    var updated = repository.Update(recipe);
    if (updated == null)
    {
      return NotFound();
    }

    return Ok(new RecipeDto(
      updated.Id,
      updated.Name,
      updated.Servings,
      [.. updated.Ingredients.Select(i => new RecipeIngredientDto(
        i.IngredientId,
        i.RequiredQuantity
      ))]
    ));
  }

  [HttpDelete("{id:guid}")]
  public IActionResult Delete(Guid id)
  {
    return repository.Delete(id) ? NoContent() : NotFound();
  }

  [HttpPost("reset")]
  public IActionResult Reset()
  {
    dataResetService.Reset();
    return NoContent();
  }

  private void ValidateRecipeRequest(List<RecipeIngredientDto>? ingredients)
  {
    if (ingredients == null || ingredients.Count == 0)
    {
      return;
    }

    for (var i = 0; i < ingredients.Count; i++)
    {
      var ingredient = ingredients[i];
      if (ingredient.RequiredQuantity <= 0)
      {
        ModelState.TryAddModelError(
          $"Ingredients[{i}].RequiredQuantity",
          "Required quantity must be greater than zero.");
      }
    }

    var duplicateIngredientIds = ingredients
      .Where(ingredient => ingredient.IngredientId != Guid.Empty)
      .GroupBy(ingredient => ingredient.IngredientId)
      .Where(group => group.Count() > 1)
      .Select(group => group.Key);

    foreach (var ingredientId in duplicateIngredientIds)
    {
      ModelState.TryAddModelError(
        nameof(RecipeIngredientDto.IngredientId),
        $"Ingredient '{ingredientId}' cannot appear more than once in a recipe.");
    }

    var missingIngredientIds = ingredients
      .Where(ingredient => ingredient.IngredientId != Guid.Empty)
      .Select(ingredient => ingredient.IngredientId)
      .Distinct()
      .Where(ingredientId => ingredientRepository.GetById(ingredientId) == null);

    foreach (var ingredientId in missingIngredientIds)
    {
      ModelState.TryAddModelError(
        nameof(RecipeIngredientDto.IngredientId),
        $"Ingredient '{ingredientId}' does not exist.");
    }
  }
}
