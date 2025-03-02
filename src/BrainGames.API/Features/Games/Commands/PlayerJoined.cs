using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using BrainGames.API.Models.Game;
using BrainGames.API.Persistence;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BrainGames.API.Features.Games.Commands;

public static class PlayerJoined
{
    public class Command : IRequest
    {
        public HubCallerContext Context { get; init; } = null!;
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IMemoryCache cache,
        BrainGamesDbContext dbContext,
        IHubContext<GameHub> hubContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var connectionId = request.Context.ConnectionId;
            var query = request.Context.GetHttpContext()?.Request.Query;

            if (query is not null && !query.TryGetValue("lobbyId", out var lobbyId) || lobbyId.Count != 1)
            {
                request.Context.Abort();
                logger.LogInformation("LobbyId not found in query parameters");
                return;
            }

            if (!cache.TryGetValue<Models.Game.Lobby>(lobbyId.ToString(), out var lobby) || lobby is null)
            {
                request.Context.Abort();
                logger.LogInformation("Lobby not found in cache");
                return;
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.NameIdentifier == request.Context.UserIdentifier, cancellationToken: cancellationToken);
            if (user is null)
            {
                request.Context.Abort();
                logger.LogInformation("User not found in database");
                return;
            }
        
            var newPlayer = new Player(user, connectionId);
        
            if (lobby.Players.Count == 0)
            {
                lobby.Host = newPlayer;
            }
            lobby.Players.Add(newPlayer);
        
            logger.LogInformation("Player {player} joined lobby {lobby}", newPlayer.User.NameIdentifier, lobby.Id);
        
            // Dodac tu kolejke graczy ktorzy czekają
            // i potem jak sie skonczy gra to opdala sie EndGameAsync i potem dodaje tych czekających do lobby
            await hubContext.Groups.AddToGroupAsync(connectionId, lobby.Id, cancellationToken);

            if (lobby.Status == LobbyStatusEnum.WaitingForStart)
            {
                await hubContext.Clients.Group(lobby.Id).SendAsync("HandleGameCommand", "LobbyUpdated", lobby,
                    cancellationToken: cancellationToken);
            }
            else
            {
                await hubContext.Clients.Client(request.Context.ConnectionId)
                    .SendAsync("HandleGameCommand", "WaitForGameToEnd", cancellationToken: cancellationToken);
            }
        }
    }
}