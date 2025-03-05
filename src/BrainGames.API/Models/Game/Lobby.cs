using BrainGames.API.Common.Constants;

namespace BrainGames.API.Models.Game;

public class Lobby
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public List<Player> Players { get; init; } = [];
    public LobbyStatusEnum Status { get; set; } = LobbyStatusEnum.WaitingForStart;
    public Player? Host { get; set; }
    public string? ActiveGameId { get; set; }
    public List<string> HistoryGamesId { get; set; } = [];

    public void PrepareForNewGame(string newGameId)
    {
        Status = LobbyStatusEnum.LoadingScreen;
        ActiveGameId = newGameId;
    }
    
    public void SetStatus(LobbyStatusEnum status)
    {
        Status = status;
    }
}
    