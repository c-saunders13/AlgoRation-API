using AlgoRationsAPI.Data;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Repositories;
using AlgoRationsAPI.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
    };
});

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

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://httpstatuses.com/500",
            Detail = app.Environment.IsDevelopment()
                ? exceptionFeature?.Error.Message
                : null,
            Instance = context.Request.Path,
        };

        var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        var wasHandled = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = problemDetails,
            Exception = exceptionFeature?.Error
        });

        if (!wasHandled)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    });
});

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
