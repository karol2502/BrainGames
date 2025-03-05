using System.Text.Json;
using BrainGames.API.Cache;
using BrainGames.API.Services;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;

namespace BrainGames.API.Features.Games.Commands;

public static class GameAction
{
    public class Command : IRequest
    {
        public HubCallerContext CallerContext { get; init; } = null!;
        public JsonElement Payload { get; init; }
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IGameActionHandler gameActionHandler,
        IDistributedCache cache) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var httpContext = request.CallerContext.GetHttpContext()
                ?? throw new InvalidOperationException("HttpContext not found in CallerContext");
            httpContext.Request.Query.TryGetValue("lobbyId", out var lobbyId );

            var lobby = await cache.GetAsync<Models.Game.Lobby>($"lobby:{lobbyId.ToString()}", cancellationToken);
            if (lobby is null)
            {
                request.CallerContext.Abort();
                logger.LogInformation("Lobby not found in cache");
                return;
            }

            if (lobby.ActiveGameId is null)
            {
                logger.LogInformation("No active game in lobby {lobby}", lobby.Id);
                return;
            }

            await gameActionHandler.ExecuteGameActionAsync(lobby.ActiveGameId, request.Payload, request.CallerContext, cancellationToken);
        }
    }
}