using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlgoRationsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController(
  IRecipeService recipeService,
  IDataResetService dataResetService) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<RecipeDto>>> GetAll()
  {
    var recipes = await recipeService.GetAllAsync();

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
    var recipe = await recipeService.GetByIdAsync(id);
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
    if (!ModelState.IsValid)
    {
      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    var createResult = await recipeService.CreateAsync(request);
    if (createResult.HasValidationErrors)
    {
      foreach (var validationError in createResult.ValidationErrors)
      {
        foreach (var message in validationError.Value)
        {
          ModelState.TryAddModelError(validationError.Key, message);
        }
      }

      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    var recipe = createResult.Recipe!;

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
    if (!ModelState.IsValid)
    {
      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    var updateResult = await recipeService.UpdateAsync(id, request);
    if (updateResult.HasValidationErrors)
    {
      foreach (var validationError in updateResult.ValidationErrors)
      {
        foreach (var message in validationError.Value)
        {
          ModelState.TryAddModelError(validationError.Key, message);
        }
      }

      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    if (updateResult.NotFound)
    {
      return NotFound();
    }

    var updated = updateResult.Recipe!;

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
    return await recipeService.DeleteAsync(id) ? NoContent() : NotFound();
  }

  [HttpPost("reset")]
  public async Task<IActionResult> Reset()
  {
    await dataResetService.ResetAsync();
    return NoContent();
  }
}
