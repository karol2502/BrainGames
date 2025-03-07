using System.Text.Json;
using System.Text.Json.Serialization;
using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using BrainGames.API.Hubs.Extensions;
using BrainGames.API.Models.Game;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using BrainGames.API.Cache;
using BrainGames.API.Services;
using Hangfire;

namespace BrainGames.API.Games;

internal sealed class ArithmeticGame : Game
{
    public override string GameName { get; init; } = nameof(ArithmeticGame);
    public override string GameDescription { get; init; } = "Answer arithmetic questions to earn points";

    public override string LobbyId { get; init; } = null!;

    // in seconds
    public int GameDuration { get; init; } = 60;
    public int GameLoadingScreenDuration { get; init; } = 3;
    public State GameState { get; init; } = null!;
    public int MaxNumberToCalculate { get; init; } = 200;


    public class State
    {
        public Dictionary<string, PlayerGameState> Players { get; set; } = new();
        public List<Round> Rounds { get; init; } = [];
    }

    public class PlayerGameState
    {
        public int Score { get; set; }
        public int CurrentRound { get; set; }
    }

    public class Round
    {
        public int RoundNumber { get; set; }
        public string Question { get; set; } = null!;
        public int Answer { get; set; }
    }


    private class ArithmeticGamePayload
    {
        [JsonPropertyName("answer")] public string Answer { get; set; } = null!;
    }

    public static async Task<ArithmeticGame> CreateAsync(Lobby lobby, ILogger logger, IHubContext<GameHub> hubContext,
        IDistributedCache cache, CancellationToken cancellationToken)
    {
        var newGame = new ArithmeticGame
        {
            LobbyId = lobby.Id,
            GameState = new State
            {
                Players = lobby.Players.ToDictionary(p => p.User.NameIdentifier, _ =>
                    new PlayerGameState
                    {
                        CurrentRound = 1,
                        Score = 0
                    }),
                Rounds = []
            }
        };

        await cache.SetAsync($"game:{newGame.Id}", newGame, cancellationToken);
        logger.LogInformation("New ArithmeticGame: {GameId} for lobby: {Lobby} created", newGame.Id, lobby.Id);

        return newGame;
    }

    public override async Task PrepareGameAsync(ILogger logger, IHubContext<GameHub> hubContext,
        IGameActionHandler gameActionHandler, CancellationToken cancellationToken)
    {
        await hubContext.SendGameCommandToAllAsync(LobbyId, "GameStarting",
            new { LoadingScreenDuration = GameLoadingScreenDuration, GameName, GameDescription }, cancellationToken);
        logger.LogInformation("Game in lobby {LobbyId} is starting", LobbyId);

        BackgroundJob.Schedule(() => gameActionHandler.StartGameAsync(Id, cancellationToken),
            TimeSpan.FromSeconds(GameLoadingScreenDuration));
    }

    public override async Task StartAsync(ILogger logger, IHubContext<GameHub> hubContext, IDistributedCache cache,
        IGameActionHandler gameActionHandler, CancellationToken cancellationToken)
    {
        var newRound = GenerateRound();
        Status = GameStatusEnum.InProgress;

        await cache.SetAsync($"game:{Id}", this, cancellationToken);

        await hubContext.SendGameCommandToAllAsync(LobbyId, "GameStarted",
            new { newRound.RoundNumber, newRound.Question, GameDuration }, cancellationToken);
        logger.LogInformation("Game: {GameName} in lobby {LobbyId} has started", GameName, LobbyId);

        BackgroundJob.Schedule(() => gameActionHandler.StopGameAsync(Id, cancellationToken),
            TimeSpan.FromSeconds(GameDuration));
    }

    public override async Task ExecuteActionAsync(JsonElement payload, IHubContext<GameHub> hubContext,
        HubCallerContext hubCallerContext, ILogger logger, IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        var typedPayload = payload.Deserialize<ArithmeticGamePayload>();
        if (typedPayload is null)
        {
            logger.LogInformation("Invalid payload, User: {User}, Lobby: {LobbyId}", hubCallerContext.UserIdentifier,
                LobbyId);
            return;
        }

        // Check if the player is in the game
        if (!GameState.Players.TryGetValue(hubCallerContext.UserIdentifier!, out var value))
        {
            hubCallerContext.Abort();
            logger.LogInformation("Player not found in game");
            return;
        }

        // Check answer
        if (int.Parse(typedPayload.Answer) == GameState.Rounds[value.CurrentRound - 1].Answer)
        {
            value.Score += 1;
            var newRoundNumber = ++value.CurrentRound;

            // Check if new round is needed
            if (newRoundNumber > GameState.Rounds.Count)
            {
                GenerateRound();
            }

            // Send new round to player
            var newRound = GameState.Rounds[newRoundNumber - 1];
            await hubContext.SendGameCommandToPlayerAsync("GameUpdated", new
            {
                newRound.RoundNumber, newRound.Question
            }, hubCallerContext.ConnectionId, cancellationToken);

            logger.LogInformation("Player {Player} answered correctly", hubCallerContext.UserIdentifier);
            await cache.SetAsync($"game:{Id}", this, cancellationToken);
        }
    }

    public override async Task StopAsync(ILogger logger, IHubContext<GameHub> hubContext, IDistributedCache cache,
        CancellationToken cancellationToken)
    {
        Status = GameStatusEnum.Finished;
        await hubContext.SendGameCommandToAllAsync(LobbyId, "GameEnded", GameState.Players,
            cancellationToken: cancellationToken);
        logger.LogInformation("Game: {GameName} in lobby {LobbyId} has ended", GameName, LobbyId);

        await cache.SetAsync($"game:{Id}", this, cancellationToken);
    }

    private Round GenerateRound()
    {
        var random = new Random();
        var operation = random.Next(0, 4); // 0: addition, 1: subtraction, 2: multiplication, 3: division
        
        Round newRound = new() { RoundNumber = GameState.Rounds.Count + 1, };

        int num1, num2;

        switch (operation)
        {
            case 0:
                num1 = random.Next(1, MaxNumberToCalculate/2);
                num2 = random.Next(1, MaxNumberToCalculate - num1);

                newRound.Question = $"{num1} + {num2}";
                newRound.Answer = num1 + num2;
                break;
            case 1:
                num1 = random.Next(1, MaxNumberToCalculate);
                num2 = random.Next(1, num1);

                newRound.Question = $"{num1} - {num2}";
                newRound.Answer = num1 - num2;
                break;
            case 2:
                num1 = random.Next(1, MaxNumberToCalculate/10);
                num2 = random.Next(1, MaxNumberToCalculate / num1);

                newRound.Question = $"{num1} * {num2}";
                newRound.Answer = num1 * num2;
                break;
            case 3:
                num1 = random.Next(1, MaxNumberToCalculate/10);
                num2 = random.Next(1, MaxNumberToCalculate / num1);

                var dividend = num1 * num2;
                newRound.Question = $"{dividend} / {num1}";
                newRound.Answer = num2;
                break;
        }

        GameState.Rounds.Add(newRound);
        return newRound;
    }
}