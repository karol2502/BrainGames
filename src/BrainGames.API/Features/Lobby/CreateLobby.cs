using BrainGames.API.Cache;
using BrainGames.API.Games;
using BrainGames.API.Models.Game;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;


namespace BrainGames.API.Features.Lobby;

public static class CreateLobby
{
    public class Command : IRequest<string>;
    
    internal sealed class Handler(ILogger<Handler> logger, IDistributedCache cache) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            var lobby = new Models.Game.Lobby();
            logger.LogInformation("Created lobby: {@lobby}", lobby);
            
            await cache.SetAsync($"lobby:{lobby.Id}", lobby, cancellationToken);
            return lobby.Id;
        }
    }
}