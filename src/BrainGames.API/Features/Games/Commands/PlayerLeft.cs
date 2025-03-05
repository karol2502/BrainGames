using BrainGames.API.Cache;
using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;


namespace BrainGames.API.Features.Games.Commands;

public static class PlayerLeft
{
    public class Command : IRequest
    {
        public HubCallerContext Context { get; init; } = null!;
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IDistributedCache cache,
        IHubContext<GameHub> hubContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var httpContext = request.Context.GetHttpContext()
                              ?? throw new InvalidOperationException("HttpContext not found in CallerContext");
            httpContext.Request.Query.TryGetValue("lobbyId", out var lobbyId );
        
            logger.LogInformation("Player {player} left lobby {lobby}", request.Context.UserIdentifier, lobbyId);
            var lobby = await cache.GetAsync<Models.Game.Lobby>($"lobby:{lobbyId.ToString()}", cancellationToken);
            if (lobby is null)
            {
                request.Context.Abort();
                logger.LogInformation("Lobby not found in cache");
                return;
            }
            lobby.Players.RemoveAll(x => x.User.NameIdentifier == request.Context.UserIdentifier);
            await hubContext.Groups.RemoveFromGroupAsync(request.Context.ConnectionId, lobby.Id, cancellationToken);

            if (lobby.Status == LobbyStatusEnum.WaitingForStart)
            {
                await hubContext.Clients.Group(lobby.Id).SendAsync("HandleGameCommand", "LobbyUpdated", lobby, cancellationToken: cancellationToken);
            }
        }
    }
}