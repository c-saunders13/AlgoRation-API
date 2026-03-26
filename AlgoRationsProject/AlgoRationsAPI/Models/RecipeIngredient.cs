namespace AlgoRationsAPI.Models;

public class RecipeIngredient
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public int Quantity { get; set; }
}
