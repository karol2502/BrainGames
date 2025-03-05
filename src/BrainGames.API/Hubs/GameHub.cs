using System.Text.Json;
using BrainGames.API.Features.Games;
using BrainGames.API.Features.Games.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace BrainGames.API.Hubs;

[Authorize]
public class GameHub(IMediator mediator) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await mediator.Send(new PlayerJoined.Command { Context = Context });
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await mediator.Send(new PlayerLeft.Command { Context = Context });
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task HandleGameCommand(string command, JsonElement payload)
    {
        var request = GameCommandMapper.FromValue(command, payload, Context);
        await mediator.Send(request);
    }
}

