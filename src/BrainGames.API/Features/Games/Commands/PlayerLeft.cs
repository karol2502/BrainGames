using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace BrainGames.API.Features.Games.Commands;

public static class PlayerLeft
{
    public class Command : IRequest
    {
        public HubCallerContext Context { get; init; } = null!;
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IMemoryCache cache,
        IHubContext<GameHub> hubContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            request.Context.GetHttpContext()?.Request.Query.TryGetValue("lobbyId", out var lobbyId);
        
            logger.LogInformation("Player {player} left lobby {lobby}", request.Context.UserIdentifier, lobbyId);
            if (!cache.TryGetValue<Models.Game.Lobby>(lobbyId.ToString(), out var lobby) || lobby is null)
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