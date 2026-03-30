using AlgoRationsAPI.Data;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Repositories;
using AlgoRationsAPI.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
      policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddDbContext<AlgoRationsDbContext>(options =>
    options.UseInMemoryDatabase("AlgoRationsDb"));

builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IRationsService, RationsService>();
builder.Services.AddScoped<IDataResetService, DataResetService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(
        scope.ServiceProvider.GetRequiredService<IIngredientRepository>(),
        scope.ServiceProvider.GetRequiredService<IRecipeRepository>()
    );
}

app.Run();
