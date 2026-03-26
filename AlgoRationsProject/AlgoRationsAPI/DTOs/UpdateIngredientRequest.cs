namespace AlgoRationsAPI.DTOs;

public record UpdateIngredientRequest(
    string Name,
    int AvailableQuantity
);
