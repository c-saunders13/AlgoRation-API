namespace AlgoRationsAPI.DTOs;

public record RationsResult(
  int TotalPeopleFed,
  List<RecipeRationBreakdown> Breakdown
);

public class RecipeRationBreakdown
{
  public Guid RecipeId { get; set; }
  public required string RecipeName { get; set; }
  public int ServingsMade { get; set; }
  public int PeopleFed { get; set; }
}
// public record RecipeRationBreakdown(
//   Guid RecipeId,
//   string RecipeName,
//   int ServingsMade,
//   int PeopleFed
// );
