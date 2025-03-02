using System.Text.Json;
using BrainGames.API.Features.Games.Commands;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BrainGames.API.Features.Games;

public static class GameCommandMapper
{
    public static IRequest FromValue(string value, JsonElement payload, HubCallerContext hubContext)
    {
        return value switch
        {
            "StartGame" => new StartGame.Command { Payload = payload.Deserialize<StartGame.StartGamePayload>() ?? throw new JsonException("Invalid payload"), Context = hubContext},
            "GameAction" => new GameAction.Command { Payload = payload, Context = hubContext},
            _ => throw new ArgumentException($"Unknown command: {value}")
        };
    }
}