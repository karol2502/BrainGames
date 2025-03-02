using BrainGames.API.Features.Games.Queries;
using BrainGames.API.Features.Lobby;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BrainGames.API.Controllers
{
    [Route("api/lobby")]
    [Authorize]
    [ApiController]
    public class LobbyController(IMediator mediator) : ControllerBase
    {
        [HttpPost("create-lobby")]
        public async Task<IActionResult> CreateLobby()
        {
            var lobbyId = await mediator.Send(new CreateLobby.Command());
            return Ok(lobbyId);
        }
        
        [HttpGet("games")]
        public async Task<IActionResult> GetActiveGames()
        {
            var games = await mediator.Send(new GetActiveGames.Query());
            return Ok(games);
        }
    }
}
