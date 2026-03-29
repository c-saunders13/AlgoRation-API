using System.ComponentModel.DataAnnotations;

namespace AlgoRationsAPI.DTOs;

public record UpdateIngredientRequest(
    [property: Required]
    [property: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    string Name,
    [property: Range(0, int.MaxValue, ErrorMessage = "Available quantity cannot be negative.")]
    int AvailableQuantity
);
