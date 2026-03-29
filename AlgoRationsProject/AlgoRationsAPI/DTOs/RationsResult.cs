using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.DTOs;

public record RationsResult(
  int TotalPeopleFed,
  List<RecipeRationBreakdown> Breakdown,
  List<Ingredient> LeftoverIngredients
);

public class RecipeRationBreakdown
{
  public Guid RecipeId { get; set; }
  public required string RecipeName { get; set; }
  public int ServingsMade { get; set; }
  public int PeopleFed { get; set; }
}

