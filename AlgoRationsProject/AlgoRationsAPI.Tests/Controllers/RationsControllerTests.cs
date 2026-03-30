using AlgoRationsAPI.Controllers;
using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using AlgoRationsAPI.Models;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AlgoRationsAPI.Tests.Controllers;

public class RationsControllerTests
{
  [Fact]
  public async Task Calculate_ReturnsOk_WithServiceResult()
  {
    var service = Substitute.For<IRationsService>();
    var expected = new RationsResult(
      8,
      [
        new RecipeRationBreakdown
        {
          RecipeId = Guid.NewGuid(),
          RecipeName = "Pizza",
          ServingsMade = 2,
          PeopleFed = 8
        }
      ],
      [
        new Ingredient
        {
          Id = Guid.NewGuid(),
          Name = "Cheese",
          AvailableQuantity = 0
        }
      ]);

    service.CalculateMaxPeopleFedAsync().Returns(Task.FromResult(expected));
    var controller = new RationsController(service);

    var result = await controller.Calculate();

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var actual = Assert.IsType<RationsResult>(ok.Value);
    Assert.Equal(expected.TotalPeopleFed, actual.TotalPeopleFed);
    Assert.Single(actual.Breakdown);
    Assert.Equal("Pizza", actual.Breakdown[0].RecipeName);
  }
}
