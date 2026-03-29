using System.ComponentModel.DataAnnotations;

namespace AlgoRationsAPI.DTOs;

public record CreateRecipeRequest(
    [param: Required]
    [param: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
        string Name,
    [param: Range(1, int.MaxValue, ErrorMessage = "Servings must be greater than zero.")]
        int Servings,
    [param: Required]
    [param: MinLength(1, ErrorMessage = "At least one ingredient is required.")]
        List<RecipeIngredientDto> Ingredients
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Ingredients == null)
        {
            yield break;
        }

        for (var i = 0; i < Ingredients.Count; i++)
        {
            if (Ingredients[i].IngredientId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "IngredientId is required.",
                    [$"Ingredients[{i}].IngredientId"]
                );
            }
        }
    }
}
