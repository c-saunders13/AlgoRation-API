using AlgoRationsAPI.Data;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Repositories;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
      policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddSingleton<IIngredientRepository, IngredientRepository>();
builder.Services.AddSingleton<IRecipeRepository, RecipeRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

DataSeeder.Seed(
    app.Services.GetRequiredService<IIngredientRepository>(),
    app.Services.GetRequiredService<IRecipeRepository>()
);

app.Run();
