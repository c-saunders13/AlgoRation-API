namespace AlgoRationsAPI.Models;

public class Recipe
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public int Servings { get; set; }
  public List<RecipeIngredient> Ingredients { get; set; } = [];
}
