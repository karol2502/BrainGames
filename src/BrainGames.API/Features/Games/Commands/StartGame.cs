using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace BrainGames.API.Features.Games.Commands;

public static class StartGame
{
    public class Command : IRequest
    {
        public HubCallerContext Context { get; init; } = null!;
        public StartGamePayload Payload { get; init; } = null!;
    }

    public class StartGamePayload
    {
        [JsonPropertyName("gameName")]
        public string GameName { get; set; } = null!;
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IMemoryCache cache,
        IServiceProvider serviceProvider) : IRequestHandler<Command>
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
            
            // Check if user is host
            if (lobby.Host is not null && lobby.Host.User.NameIdentifier != request.Context.UserIdentifier)
            {
                logger.LogInformation("User is not host of lobby {lobby}", lobby.Id);
                return;
            }

            var gameName = request.Payload.GameName == "Random"
                ? GameFactory.GameNames[new Random().Next(GameFactory.GameNames.Count)]
                : request.Payload.GameName;
            
            var newGame = GameFactory.CreateGame(gameName, lobby, serviceProvider);
            await lobby.StartNewGame(newGame, cancellationToken);
        }
    }
}