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
  public async Task<ActionResult<IEnumerable<RecipeDto>>> GetAll()
  {
    var recipes = await repository.GetAllAsync();

    return Ok(recipes.Select(recipe => new RecipeDto(
        recipe.Id,
        recipe.Name,
        recipe.Servings,
        [.. recipe.Ingredients.Select(i => new RecipeIngredientDto(
          i.IngredientId,
          i.RequiredQuantity
        ))]
      )));
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<RecipeDto>> GetById(Guid id)
  {
    var recipe = await repository.GetByIdAsync(id);
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
  public async Task<ActionResult<RecipeDto>> Create(CreateRecipeRequest request)
  {
    await ValidateRecipeRequestAsync(request.Ingredients);
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

    await repository.AddAsync(recipe);
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
  public async Task<ActionResult<RecipeDto>> Update(Guid id, UpdateRecipeRequest request)
  {
    await ValidateRecipeRequestAsync(request.Ingredients);
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

    var updated = await repository.UpdateAsync(recipe);
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
  public async Task<IActionResult> Delete(Guid id)
  {
    return await repository.DeleteAsync(id) ? NoContent() : NotFound();
  }

  [HttpPost("reset")]
  public async Task<IActionResult> Reset()
  {
    await dataResetService.ResetAsync();
    return NoContent();
  }

  private async Task ValidateRecipeRequestAsync(List<RecipeIngredientDto>? ingredients)
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
      .ToList();

    var ingredientChecks = await Task.WhenAll(missingIngredientIds.Select(async ingredientId => new
    {
      IngredientId = ingredientId,
      Exists = await ingredientRepository.GetByIdAsync(ingredientId) != null
    }));

    foreach (var ingredientId in ingredientChecks.Where(check => !check.Exists).Select(check => check.IngredientId))
    {
      ModelState.TryAddModelError(
        nameof(RecipeIngredientDto.IngredientId),
        $"Ingredient '{ingredientId}' does not exist.");
    }
  }
}
