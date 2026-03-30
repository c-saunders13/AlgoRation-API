using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Services;

public sealed class RecipeMutationResult
{
  private RecipeMutationResult(Recipe? recipe, bool notFound, Dictionary<string, string[]> validationErrors)
  {
    Recipe = recipe;
    NotFound = notFound;
    ValidationErrors = validationErrors;
  }

  public Recipe? Recipe { get; }
  public bool NotFound { get; }
  public IReadOnlyDictionary<string, string[]> ValidationErrors { get; }
  public bool HasValidationErrors => ValidationErrors.Count > 0;

  public static RecipeMutationResult Success(Recipe recipe) => new(recipe, false, []);

  public static RecipeMutationResult NotFoundResult() => new(null, true, []);

  public static RecipeMutationResult ValidationFailed(Dictionary<string, string[]> validationErrors) =>
    new(null, false, validationErrors);
}