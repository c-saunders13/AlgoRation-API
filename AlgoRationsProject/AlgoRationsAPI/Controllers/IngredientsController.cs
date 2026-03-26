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
  public ActionResult<IEnumerable<IngredientDto>> GetAll()
  {
    return Ok(repository.GetAll().Select(ingredient => new IngredientDto(
      ingredient.Id,
      ingredient.Name,
      ingredient.AvailableQuantity
    )));
  }

  [HttpGet("{id:guid}")]
  public ActionResult<IngredientDto> GetById(Guid id)
  {
    var ingredient = repository.GetById(id);
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
  public ActionResult<IngredientDto> Create(CreateIngredientRequest request)
  {
    var ingredient = new Ingredient
    {
      Name = request.Name,
      AvailableQuantity = request.AvailableQuantity,
    };

    var created = repository.Add(ingredient);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, new IngredientDto(
      created.Id,
      created.Name,
      created.AvailableQuantity
    ));
  }

  [HttpPut("{id:guid}")]
  public ActionResult<IngredientDto> Update(Guid id, UpdateIngredientRequest request)
  {
    var ingredient = new Ingredient
    {
      Id = id,
      Name = request.Name,
      AvailableQuantity = request.AvailableQuantity,
    };

    var updated = repository.Update(ingredient);
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
  public IActionResult Delete(Guid id)
  {
    return repository.Delete(id) ? NoContent() : NotFound();
  }
}
