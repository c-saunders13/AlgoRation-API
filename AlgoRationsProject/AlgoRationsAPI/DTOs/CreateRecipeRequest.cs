namespace AlgoRationsAPI.DTOs;

public record CreateRecipeRequest(
    string Name,
    int Servings,
    List<RecipeIngredientDto> Ingredients
);
