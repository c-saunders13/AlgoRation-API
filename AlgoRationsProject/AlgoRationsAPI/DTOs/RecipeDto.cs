namespace AlgoRationsAPI.DTOs;

public record RecipeDto(
    Guid Id,
    string Name,
    int Servings,
    List<RecipeIngredientDto> Ingredients
);
