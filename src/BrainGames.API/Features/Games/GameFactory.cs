using BrainGames.API.Games;

namespace BrainGames.API.Features.Games;

public static class GameFactory
{
    public static List<string> GameNames { get; } = typeof(ArithmeticGame).Assembly.GetTypes()
        .Where(t => t is { Namespace: "BrainGames.API.Games", IsClass: true, IsSealed: true, IsNested: false })
        .Select(t => t.Name)
        .ToList();
    
    public static Models.Game.Game CreateGame(string gameName, Models.Game.Lobby lobby, IServiceProvider serviceProvider)
    {
        var test = gameName switch
        {
            nameof(ArithmeticGame) => new ArithmeticGame(),
            _ => throw new ArgumentException($"Unknown game: {gameName}")
        };

        return test;
    }
}