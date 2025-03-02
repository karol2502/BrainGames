using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace BrainGames.API.Features.Lobby;

public static class CreateLobby
{
    public class Command : IRequest<string>;
    
    internal sealed class Handler(ILogger<Handler> logger, IMemoryCache cache) : IRequestHandler<Command, string>
    {
        public Task<string> Handle(Command request, CancellationToken cancellationToken)
        {
            var lobby = new Models.Game.Lobby();
            cache.Set(lobby.Id, lobby);
            
            logger.LogInformation("Created lobby: {@lobby}", lobby);
            return Task.FromResult(lobby.Id);
        }
    }
}