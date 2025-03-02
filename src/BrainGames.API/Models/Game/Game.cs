using System.Text.Json;
using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BrainGames.API.Models.Game;

public abstract class Game(Lobby lobby, IHubContext<GameHub> hubContext)
{
    public abstract string GameName { get; }
    public GameStatusEnum Status { get; set; } = GameStatusEnum.Starting;
    public Lobby Lobby { get; } = lobby;
    public Timer? ActionTimer { get; set; }
    private IHubContext<GameHub> HubContext { get; } = hubContext;

    public abstract Task ExecuteActionAsync(JsonElement payload, HubCallerContext hubCallerContext, CancellationToken cancellationToken);

    public abstract Task StartAsync(CancellationToken cancellationToken);
    
    protected Task SendGameCommandToAllAsync<T>(string command, T payload, CancellationToken cancellationToken = default)
    {
        return HubContext.Clients.Group(Lobby.Id).SendAsync("HandleGameCommand", command, payload, cancellationToken: cancellationToken);
    }
    
    protected Task SendGameCommandToPlayerAsync<T>(string command, T payload, string connectionId, CancellationToken cancellationToken = default)
    {
        return HubContext.Clients.Client(connectionId).SendAsync("HandleGameCommand", command, payload, cancellationToken: cancellationToken);
    }
}