namespace AlgoRationsAPI.DTOs;

public record RecipeIngredientDto(
    Guid IngredientId,
    int RequiredQuantity
);
