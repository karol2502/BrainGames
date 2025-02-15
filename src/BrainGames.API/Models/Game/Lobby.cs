namespace BrainGames.API.Models.Game;

public class Lobby
{
    public string Id { get; } = Guid.NewGuid().ToString();
}