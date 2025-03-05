using BrainGames.API.Services;
using BrainGames.API.Cache;
using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using BrainGames.API.Hubs.Extensions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;

namespace BrainGames.API.Features.Games.Commands;

public static class GoBackToLobby
{
    public class Command : IRequest
    {
        public HubCallerContext CallerContext { get; init; } = null!;
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IDistributedCache cache,
        IHubContext<GameHub> hubContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var httpContext = request.CallerContext.GetHttpContext()
                              ?? throw new InvalidOperationException("HttpContext not found in CallerContext");
            httpContext.Request.Query.TryGetValue("lobbyId", out var lobbyId);

            var lobby = await cache.GetAsync<Models.Game.Lobby>($"lobby:{lobbyId.ToString()}", cancellationToken);
            if (lobby is null)
            {
                request.CallerContext.Abort();
                logger.LogInformation("Lobby not found in cache");
                return;
            }

            // Check if user is host
            if (lobby.Host is null || lobby.Host is not null &&
                lobby.Host.User.NameIdentifier != request.CallerContext.UserIdentifier)
            {
                logger.LogInformation("User is not host of lobby {lobby}", lobby.Id);
                return;
            }

            if (lobby.ActiveGameId is not null)
            {
                lobby.HistoryGamesId.Add(lobby.ActiveGameId);
                lobby.ActiveGameId = null;
            }

            lobby.Status = LobbyStatusEnum.WaitingForStart;
            await cache.SetAsync($"lobby:{lobbyId.ToString()}", lobby, cancellationToken);

            await hubContext.SendGameCommandToAllAsync(lobby.Id, "LobbyUpdated", lobby, cancellationToken);
        }
    }
}