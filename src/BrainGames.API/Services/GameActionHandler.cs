using System.Text.Json;
using BrainGames.API.Games;
using BrainGames.API.Hubs;
using BrainGames.API.Models.Game;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.AspNetCore.SignalR;

namespace BrainGames.API.Services;

public interface IGameActionHandler
{
    Task<string> CreateGameAsync(Lobby lobby, string gameName, CancellationToken cancellationToken);
    Task StartGameAsync(string gameId, CancellationToken cancellationToken);

    Task ExecuteGameActionAsync(string gameId, JsonElement payload, HubCallerContext callerContext,
        CancellationToken cancellationToken);

    Task StopGameAsync(string gameId, CancellationToken cancellationToken);
}

public sealed class GameActionHandler(
    ILogger<GameActionHandler> logger,
    IDistributedCache cache,
    IHubContext<GameHub> hubContext) : IGameActionHandler
{
    public async Task<string> CreateGameAsync(Lobby lobby, string gameName, CancellationToken cancellationToken)
    {
        switch (gameName)
        {
            case nameof(ArithmeticGame):
                var arithmeticGame =
                    await ArithmeticGame.CreateAsync(lobby, logger, hubContext, cache, cancellationToken);
                await arithmeticGame.PrepareGameAsync(logger, hubContext, this, cancellationToken);
                return arithmeticGame.Id;
            default:
                throw new ArgumentException($"Unknown game: {gameName}");
        }
    }

    public async Task StartGameAsync(string gameId, CancellationToken cancellationToken = default)
    {
        var cachedGame = await cache.GetStringAsync($"game:{gameId}", cancellationToken)
                         ?? throw new Exception("Game not found in cache");

        // TODO: Fix deserialize type
        var game = JsonSerializer.Deserialize<ArithmeticGame>(cachedGame)
                   ?? throw new Exception("Failed to deserialize game");

        switch (game.GameName)
        {
            case nameof(ArithmeticGame):
                var arithmeticGame = JsonSerializer.Deserialize<ArithmeticGame>(cachedGame)
                                     ?? throw new Exception("Failed to deserialize game");
                await arithmeticGame.StartAsync(logger, hubContext, cache, this, cancellationToken);
                break;
            default:
                throw new Exception("Unknown game");
        }
    }

    public async Task ExecuteGameActionAsync(string gameId, JsonElement payload, HubCallerContext callerContext,
        CancellationToken cancellationToken)
    {
        var cachedSerializedGame = await cache.GetStringAsync($"game:{gameId}", cancellationToken)
                                   ?? throw new Exception("Game not found in cache");

        var game = JsonSerializer.Deserialize<ArithmeticGame>(cachedSerializedGame)
                   ?? throw new Exception("Failed to deserialize game");

        switch (game.GameName)
        {
            case nameof(ArithmeticGame):
                var arithmeticGame = JsonSerializer.Deserialize<ArithmeticGame>(cachedSerializedGame)
                                     ?? throw new Exception("Failed to deserialize game");
                await arithmeticGame.ExecuteActionAsync(payload, hubContext, callerContext, logger, cache,
                    cancellationToken);
                break;
            default:
                throw new Exception("Unknown game");
        }
    }

    public async Task StopGameAsync(string gameId, CancellationToken cancellationToken = default)
    {
        var cachedSerializedGame = await cache.GetStringAsync($"game:{gameId}", cancellationToken)
                                   ?? throw new Exception("Game not found in cache");

        var game = JsonSerializer.Deserialize<ArithmeticGame>(cachedSerializedGame)
                   ?? throw new Exception("Failed to deserialize game");

        switch (game.GameName)
        {
            case nameof(ArithmeticGame):
                var arithmeticGame = JsonSerializer.Deserialize<ArithmeticGame>(cachedSerializedGame)
                                     ?? throw new Exception("Failed to deserialize game");
                await arithmeticGame.StopAsync(logger, hubContext, cache, cancellationToken);
                break;
            default:
                throw new Exception("Unknown game");
        }
    }
}