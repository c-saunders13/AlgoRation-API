namespace AlgoRationsAPI.DTOs;

public record IngredientDto(
  Guid Id,
  string Name,
  int AvailableQuantity
);
