namespace AlgoRationsAPI.Models;

public class Ingredient
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public int AvailableQuantity { get; set; }
}
