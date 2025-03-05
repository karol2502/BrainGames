using System.Text.Json;
using System.Text.Json.Serialization;
using BrainGames.API.Cache;
using BrainGames.API.Services;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;


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
        [JsonPropertyName("gameName")] public string GameName { get; set; } = null!;
    }

    internal sealed class Handler(
        ILogger<Handler> logger,
        IDistributedCache cache,
        IGameActionHandler gameActionHandler) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var httpContext = request.Context.GetHttpContext()
                              ?? throw new InvalidOperationException("HttpContext not found in CallerContext");
            httpContext.Request.Query.TryGetValue("lobbyId", out var lobbyId);

            var lobby = await cache.GetAsync<Models.Game.Lobby>($"lobby:{lobbyId.ToString()}", cancellationToken);
            if (lobby is null)
            {
                request.Context.Abort();
                logger.LogInformation("Lobby not found in cache");
                return;
            }

            // Check if user is host
            if (lobby.Host is null || lobby.Host is not null &&
                lobby.Host.User.NameIdentifier != request.Context.UserIdentifier)
            {
                logger.LogInformation("User is not host of lobby {lobby}", lobby.Id);
                return;
            }

            var gameName = request.Payload.GameName == "Random"
                ? GameFactory.GameNames[new Random().Next(GameFactory.GameNames.Count)]
                : request.Payload.GameName;

            var newGameId = await gameActionHandler.CreateGameAsync(lobby, gameName, cancellationToken);

            lobby.PrepareForNewGame(newGameId);
            await cache.SetAsync($"lobby:{lobbyId.ToString()}", lobby, cancellationToken);
        }
    }
}