using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainGames.API.Controllers;

[Route("api/test")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("anon")]
    public IActionResult Anon()
    {
        return Ok(new {response = "works"});
    }
    
    [HttpGet("authorize")]
    [Authorize]
    public IActionResult Authorize()
    {
        return Ok(new {response = "works"});
    }
}