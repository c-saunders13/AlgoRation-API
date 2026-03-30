using AlgoRationsAPI.DTOs;
using AlgoRationsAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AlgoRationsAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RationsController(IRationsService rationsService) : ControllerBase
{
  [HttpGet("calculate")]
  public async Task<ActionResult<RationsResult>> Calculate() =>
    Ok(await rationsService.CalculateMaxPeopleFedAsync());
}
