using System.ComponentModel.DataAnnotations;

namespace AlgoRationsAPI.DTOs;

public record RecipeIngredientDto(
    Guid IngredientId,
    [property: Range(1, int.MaxValue, ErrorMessage = "Required quantity must be greater than zero.")]
    int RequiredQuantity
);
