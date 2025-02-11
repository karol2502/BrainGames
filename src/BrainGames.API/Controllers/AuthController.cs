using Microsoft.AspNetCore.Mvc;

namespace BrainGames.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController  : ControllerBase
{
    [HttpPost("google")]
    public async Task<IActionResult> ExchangeCode()
    {
        return Ok();
    }
}