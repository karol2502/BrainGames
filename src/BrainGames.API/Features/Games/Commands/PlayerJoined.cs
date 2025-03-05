using BrainGames.API.Cache;
using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using BrainGames.API.Models.Game;
using BrainGames.API.Persistence;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;


namespace BrainGames.API.Features.Games.Commands;

public static class PlayerJoined
{
    public class Command : IRequest
    {
        public HubCallerContext Context { get; init; } = null!;
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IDistributedCache cache,
        BrainGamesDbContext dbContext,
        IHubContext<GameHub> hubContext) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var connectionId = request.Context.ConnectionId;
            var httpContext = request.Context.GetHttpContext()
                              ?? throw new InvalidOperationException("HttpContext not found in CallerContext");
            httpContext.Request.Query.TryGetValue("lobbyId", out var lobbyId );

            var lobby = await cache.GetAsync<Models.Game.Lobby>($"lobby:{lobbyId.ToString()}", cancellationToken);
            if (lobby is null)
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
            
            await cache.SetAsync($"lobby:{lobbyId.ToString()}", lobby, cancellationToken);
        
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