using System.IdentityModel.Tokens.Jwt;
using BrainGames.API.Features.User;
using BrainGames.API.Models;
using BrainGames.API.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainGames.API.Controllers;

[Route("api/auth")]
[ApiController]
[Authorize]
public class AuthController(IMediator mediator)  : ControllerBase
{
    [HttpPost("oauth2")]
    public async Task<IActionResult> CheckIfUserExists([FromBody] IdToken idToken)
    {
        await mediator.Send(new CheckIfUserExistsOrCreate.Command(idToken));
        return Ok();
    }
}
