using BrainGames.API.Common.Constants;

namespace BrainGames.API.Models.Game;

public class Lobby
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public List<Player> Players { get; } = [];
    public LobbyStatusEnum Status { get; private set; } = LobbyStatusEnum.WaitingForStart;
    public Player? Host { get; set; }
    public Game? ActiveGame { get; private set; }
    public List<Game> GamesHistory { get; } = [];

    public async Task StartNewGame(Game newGame, CancellationToken cancellationToken)
    {
        Status = LobbyStatusEnum.LoadingScreen;
        ActiveGame = newGame;
        await newGame.StartAsync(cancellationToken);
    }
    
    public void SetStatus(LobbyStatusEnum status)
    {
        Status = status;
    }
}
    