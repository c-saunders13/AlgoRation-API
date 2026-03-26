using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;

namespace AlgoRationsAPI.Data;

public static class DataSeeder
{
  public static void Seed(IIngredientRepository ingredients, IRecipeRepository recipes)
  {
    var cucumber = ingredients.Add(new Ingredient { Name = "Cucumber", AvailableQuantity = 2 });
    var olives = ingredients.Add(new Ingredient { Name = "Olives", AvailableQuantity = 2 });
    var lettuce = ingredients.Add(new Ingredient { Name = "Lettuce", AvailableQuantity = 3 });
    var meat = ingredients.Add(new Ingredient { Name = "Meat", AvailableQuantity = 6 });
    var tomato = ingredients.Add(new Ingredient { Name = "Tomato", AvailableQuantity = 6 });
    var cheese = ingredients.Add(new Ingredient { Name = "Cheese", AvailableQuantity = 8 });
    var dough = ingredients.Add(new Ingredient { Name = "Dough", AvailableQuantity = 10 });

    recipes.Add(new Recipe
    {
      Name = "Burger",
      Servings = 1,
      Ingredients =
      [
        new() { IngredientId = meat.Id, RequiredQuantity = 1 },
        new() { IngredientId = lettuce.Id, RequiredQuantity = 1 },
        new() { IngredientId = tomato.Id, RequiredQuantity = 1 },
        new() { IngredientId = cheese.Id, RequiredQuantity = 1 },
        new() { IngredientId = dough.Id, RequiredQuantity = 1 }
      ]
    });

    recipes.Add(new Recipe
    {
      Name = "Pie",
      Servings = 1,
      Ingredients =
      [
        new() { IngredientId = dough.Id, RequiredQuantity = 2 },
        new() { IngredientId = meat.Id, RequiredQuantity = 2 }
      ]
    });

    recipes.Add(new Recipe
    {
      Name = "Sandwich",
      Servings = 1,
      Ingredients =
      [
        new() { IngredientId = dough.Id, RequiredQuantity = 1 },
        new() { IngredientId = cucumber.Id, RequiredQuantity = 1 }
      ]
    });

    recipes.Add(new Recipe
    {
      Name = "Pasta",
      Servings = 2,
      Ingredients =
      [
        new() { IngredientId = dough.Id, RequiredQuantity = 2 },
        new() { IngredientId = tomato.Id, RequiredQuantity = 1 },
        new() { IngredientId = cheese.Id, RequiredQuantity = 2 },
        new() { IngredientId = meat.Id, RequiredQuantity = 1 }
      ]
    });

    recipes.Add(new Recipe
    {
      Name = "Salad",
      Servings = 3,
      Ingredients =
      [
        new() { IngredientId = lettuce.Id, RequiredQuantity = 2 },
        new() { IngredientId = tomato.Id, RequiredQuantity = 2 },
        new() { IngredientId = cucumber.Id, RequiredQuantity = 1 },
        new() { IngredientId = cheese.Id, RequiredQuantity = 2 },
        new() { IngredientId = olives.Id, RequiredQuantity = 1 }
      ]
    });

    recipes.Add(new Recipe
    {
      Name = "Pizza",
      Servings = 4,
      Ingredients =
      [
        new() { IngredientId = dough.Id, RequiredQuantity = 3 },
        new() { IngredientId = tomato.Id, RequiredQuantity = 2 },
        new() { IngredientId = cheese.Id, RequiredQuantity = 3 },
        new() { IngredientId = olives.Id, RequiredQuantity = 1 }
      ]
    });
  }
}
