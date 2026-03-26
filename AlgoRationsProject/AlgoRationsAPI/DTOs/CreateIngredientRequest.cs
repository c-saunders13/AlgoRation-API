namespace AlgoRationsAPI.DTOs;

public record CreateIngredientRequest(
    string Name,
    int AvailableQuantity
);
