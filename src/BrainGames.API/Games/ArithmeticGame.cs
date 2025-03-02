using System.Text.Json;
using System.Text.Json.Serialization;
using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using BrainGames.API.Models.Game;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace BrainGames.API.Games;

internal sealed class ArithmeticGame(Lobby lobby, IServiceProvider serviceProvider)
    : Game(lobby, serviceProvider.GetRequiredService<IHubContext<GameHub>>())
{
    private readonly ILogger<ArithmeticGame> _logger = serviceProvider.GetRequiredService<ILogger<ArithmeticGame>>();
    private readonly IMemoryCache _cache = serviceProvider.GetRequiredService<IMemoryCache>();

    private class State
    {
        public Dictionary<string, PlayerGameState> Players { get; set; } = new();
        public List<Round> Rounds { get; init; } = [];
    }

    private class PlayerGameState
    {
        public int Score { get; set; }
        public int CurrentRound { get; set; }
    }

    private class Round
    {
        public int RoundNumber { get; set; }
        public string Question { get; set; } = null!;
        public int Answer { get; set; }
    }

    private State GameState { get; set; } = new()
    {
        Players = lobby.Players.ToDictionary(p => p.User.NameIdentifier, p => new PlayerGameState
        {
            CurrentRound = 1,
            Score = 0
        }),
        Rounds = []
    };

    public override string GameName { get; } = nameof(ArithmeticGame);
    // in seconds
    public int GameDuration { get; } = 60;
    public int GameLoadingScreenDuration { get; } = 3;
    
    
    private class ArithmeticGamePayload
    {
        [JsonPropertyName("answer")] public string Answer { get; set; } = null!;
    }

    public override async Task ExecuteActionAsync(JsonElement payload, HubCallerContext hubCallerContext, CancellationToken cancellationToken)
    {
        var typedPayload = payload.Deserialize<ArithmeticGamePayload>();
        if (typedPayload is null)
        {
            _logger.LogInformation("Invalid payload, User: {User}, Lobby: {LobbyId}", hubCallerContext.UserIdentifier, Lobby.Id);
            return;
        }
        
        hubCallerContext.GetHttpContext()?.Request.Query.TryGetValue("lobbyId", out var lobbyId);
            
        if (!_cache.TryGetValue<Models.Game.Lobby>(lobbyId.ToString(), out var lobby) || lobby is null)
        {
            hubCallerContext.Abort();
            _logger.LogInformation("Lobby not found in cache");
            return;
        }
        
        // Check if the player is in the game
        if (!GameState.Players.ContainsKey(hubCallerContext.UserIdentifier!))
        {
            hubCallerContext.Abort();
            _logger.LogInformation("Player not found in game");
            return;
        }
        
        // Check answer
        if (int.Parse(typedPayload.Answer) == GameState.Rounds[GameState.Players[hubCallerContext.UserIdentifier!].CurrentRound - 1].Answer)
        {
            GameState.Players[hubCallerContext.UserIdentifier!].Score += 1;
            var newRoundNumber = ++GameState.Players[hubCallerContext.UserIdentifier!].CurrentRound;
            
            // Check if new round is needed
            if (newRoundNumber > GameState.Rounds.Count)
            {
                GenerateRound();
            }

            // Send new round to player
            var newRound = GameState.Rounds[newRoundNumber - 1];
            await SendGameCommandToPlayerAsync("GameUpdated", new
            {
                RoundNumber = newRound.RoundNumber, Question = newRound.Question
            }, hubCallerContext.ConnectionId, cancellationToken);
            
            _logger.LogInformation("Player {Player} answered correctly", hubCallerContext.UserIdentifier);
        }
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await SendGameCommandToAllAsync("GameStarting", new { LoadingScreenDuration = GameLoadingScreenDuration, GameName }, cancellationToken);
        _logger.LogInformation("Game in lobby {LobbyId} is starting", Lobby.Id);
        
        // Start game
        BackgroundJob.Schedule( 
            () => HandleStartGame(this),
            TimeSpan.FromSeconds(GameLoadingScreenDuration));

        BackgroundJob.Schedule(
            () => HandleEndGame(),
            TimeSpan.FromSeconds(GameLoadingScreenDuration + GameDuration));
    }
    
    public async static Task HandleStartGame(ArithmeticGame game)
    {
        var newRound = game.GenerateRound();
        game.Status = GameStatusEnum.InProgress;
        game.Lobby.SetStatus(LobbyStatusEnum.InGame);
        await game.SendGameCommandToAllAsync("GameStarted", new {
            RoundNumber = newRound.RoundNumber, Question = newRound.Question, game.GameDuration });
        game._logger.LogInformation("Game: {GameName} in lobby {LobbyId} has started", game.GameName, game.Lobby.Id);
    }
    
    public async Task HandleEndGame()
    {
        Status = GameStatusEnum.Finished;
        Lobby.SetStatus(LobbyStatusEnum.Scoreboard);
        await SendGameCommandToAllAsync("GameEnded", GameState.Players);
        _logger.LogInformation("Game: {GameName} in lobby {LobbyId} has ended", GameName, Lobby.Id);
    }
    
    private Round GenerateRound()
    {
        while (true)
        {
            var random = new Random();
            int num1 = random.Next(1, 101);
            int num2 = random.Next(1, 101);
            int operation = random.Next(0, 4); // 0: addition, 1: subtraction, 2: multiplication, 3: division

            Round newRound = new() { RoundNumber = GameState.Rounds.Count + 1, };

            switch (operation)
            {
                case 0:
                    newRound.Question = $"{num1} + {num2}";
                    newRound.Answer = num1 + num2;
                    break;
                case 1:
                    newRound.Question = $"{num1} - {num2}";
                    newRound.Answer = num1 - num2;
                    break;
                case 2:
                    newRound.Question = $"{num1} * {num2}";
                    newRound.Answer = num1 * num2;
                    break;
                case 3:
                    // Ensure division results in an integer
                    while (num1 % num2 != 0)
                    {
                        num1 = random.Next(1, 101);
                        num2 = random.Next(1, 101);
                    }

                    newRound.Question = $"{num1} / {num2}";
                    newRound.Answer = num1 / num2;
                    break;
            }

            // Ensure the answer is within the range 1 to 1000
            if (newRound.Answer is < 1 or > 1000)
            {
                continue;
            }

            GameState.Rounds.Add(newRound);
            return newRound;
        }
    }
}