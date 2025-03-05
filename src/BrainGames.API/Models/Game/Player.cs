using BrainGames.API.Entities;

namespace BrainGames.API.Models.Game;

public class Player(User user, string connectionId)
{
    public User User { get; } = user;
    public string ConnectionId { get; } = connectionId;
}