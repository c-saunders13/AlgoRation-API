using AlgoRationsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoRationsAPI.Data;

public class AlgoRationsDbContext(DbContextOptions<AlgoRationsDbContext> options) : DbContext(options)
{
  public DbSet<Ingredient> Ingredients => Set<Ingredient>();
  public DbSet<Recipe> Recipes => Set<Recipe>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Ingredient>(entity =>
    {
      entity.HasKey(ingredient => ingredient.Id);
      entity.Property(ingredient => ingredient.Name).IsRequired();
    });

    modelBuilder.Entity<Recipe>(entity =>
    {
      entity.HasKey(recipe => recipe.Id);
      entity.Property(recipe => recipe.Name).IsRequired();

      entity.OwnsMany(recipe => recipe.Ingredients, recipeIngredient =>
      {
        recipeIngredient.WithOwner().HasForeignKey("RecipeId");
        recipeIngredient.HasKey("RecipeId", nameof(RecipeIngredient.IngredientId));
      });
    });
  }
}