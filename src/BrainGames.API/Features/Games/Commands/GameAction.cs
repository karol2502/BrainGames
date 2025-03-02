using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace BrainGames.API.Features.Games.Commands;

public static class GameAction
{
    public class Command : IRequest
    {
        public HubCallerContext Context { get; init; } = null!;
        public JsonElement Payload { get; init; }
    }
    
    internal sealed class Handler(
        ILogger<Handler> logger,
        IMemoryCache cache) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            request.Context.GetHttpContext()?.Request.Query.TryGetValue("lobbyId", out var lobbyId);
            
            if (!cache.TryGetValue<Models.Game.Lobby>(lobbyId.ToString(), out var lobby) || lobby is null)
            {
                request.Context.Abort();
                logger.LogInformation("Lobby not found in cache");
                return;
            }

            if (lobby.ActiveGame is null)
            {
                logger.LogInformation("No active game in lobby {lobby}", lobby.Id);
                return;
            }
            
            await lobby.ActiveGame.ExecuteActionAsync(request.Payload, request.Context,  cancellationToken);
        }
    }
}