using System.Text.Json;
using BrainGames.API.Features.Games.Commands;
using MediatR;
using Microsoft.AspNetCore.SignalR;

namespace BrainGames.API.Features.Games;

public static class GameCommandMapper
{
    public static IRequest FromValue(string value, JsonElement payload, HubCallerContext callerContext)
    {
        return value switch
        {
            "StartGame" => new StartGame.Command { Payload = payload.Deserialize<StartGame.StartGamePayload>() ?? throw new JsonException("Invalid payload"), Context = callerContext},
            "GameAction" => new GameAction.Command { Payload = payload, CallerContext = callerContext},
            "GoBackToLobby" => new GoBackToLobby.Command { CallerContext = callerContext},
            _ => throw new ArgumentException($"Unknown command: {value}")
        };
    }
}