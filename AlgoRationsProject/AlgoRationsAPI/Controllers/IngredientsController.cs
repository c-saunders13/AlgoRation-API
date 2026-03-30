using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace AlgoRationsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IngredientsController(IIngredientRepository repository) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<IEnumerable<IngredientDto>>> GetAll()
  {
    var ingredients = await repository.GetAllAsync();

    return Ok(ingredients.Select(ingredient => new IngredientDto(
      ingredient.Id,
      ingredient.Name,
      ingredient.AvailableQuantity
    )));
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<IngredientDto>> GetById(Guid id)
  {
    var ingredient = await repository.GetByIdAsync(id);
    if (ingredient == null)
    {
      return NotFound();
    }

    return Ok(new IngredientDto(
      ingredient.Id,
      ingredient.Name,
      ingredient.AvailableQuantity
    ));
  }

  [HttpPost]
  public async Task<ActionResult<IngredientDto>> Create(CreateIngredientRequest request)
  {
    if (!ModelState.IsValid)
    {
      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    var ingredient = new Ingredient
    {
      Name = request.Name,
      AvailableQuantity = request.AvailableQuantity,
    };

    var created = await repository.AddAsync(ingredient);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, new IngredientDto(
      created.Id,
      created.Name,
      created.AvailableQuantity
    ));
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult<IngredientDto>> Update(Guid id, UpdateIngredientRequest request)
  {
    if (!ModelState.IsValid)
    {
      return ValidationProblem(modelStateDictionary: ModelState, statusCode: StatusCodes.Status400BadRequest);
    }

    var ingredient = new Ingredient
    {
      Id = id,
      Name = request.Name,
      AvailableQuantity = request.AvailableQuantity,
    };

    var updated = await repository.UpdateAsync(ingredient);
    if (updated == null)
    {
      return NotFound();
    }

    return Ok(new IngredientDto(
      updated.Id,
      updated.Name,
      updated.AvailableQuantity
    ));
  }

  [HttpDelete("{id:guid}")]
  public async Task<IActionResult> Delete(Guid id)
  {
    return (await repository.DeleteAsync(id)) switch
    {
      IngredientDeleteResult.Deleted => NoContent(),
      IngredientDeleteResult.NotFound => NotFound(),
      IngredientDeleteResult.InUse => Conflict(new ErrorResponse("Ingredient is included in a recipe and cannot be deleted.")),
      _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
  }
}
