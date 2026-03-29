using System.ComponentModel.DataAnnotations;

namespace AlgoRationsAPI.DTOs;

public record UpdateIngredientRequest(
    [param: Required]
    [param: RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be empty or whitespace.")]
    string Name,
    [param: Range(0, int.MaxValue, ErrorMessage = "Available quantity cannot be negative.")]
    int AvailableQuantity
);
