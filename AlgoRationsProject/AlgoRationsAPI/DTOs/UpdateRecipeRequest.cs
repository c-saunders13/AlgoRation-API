namespace AlgoRationsAPI.DTOs;

public record UpdateRecipeRequest(
    string Name,
    int Servings,
    List<RecipeIngredientDto> Ingredients
);
