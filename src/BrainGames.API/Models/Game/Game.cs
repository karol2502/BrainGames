using System.Text.Json;
using BrainGames.API.Common.Constants;
using BrainGames.API.Hubs;
using BrainGames.API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;

namespace BrainGames.API.Models.Game;

public abstract class Game
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public abstract string GameName { get; init; }
    public abstract string GameDescription { get; init; }
    public GameStatusEnum Status { get; set; } = GameStatusEnum.Starting;
    public abstract string LobbyId { get; init; }

    public abstract Task PrepareGameAsync(ILogger logger, IHubContext<GameHub> hubContext,
        IGameActionHandler gameActionHandler, CancellationToken cancellationToken);

    public abstract Task StartAsync(ILogger logger, IHubContext<GameHub> hubContext, IDistributedCache cache,
        IGameActionHandler gameActionHandler, CancellationToken cancellationToken);

    public abstract Task ExecuteActionAsync(JsonElement payload, IHubContext<GameHub> hubContext,
        HubCallerContext hubCallerContext, ILogger logger, IDistributedCache cache,
        CancellationToken cancellationToken);

    public abstract Task StopAsync(ILogger logger, IHubContext<GameHub> hubContext, IDistributedCache cache,
        CancellationToken cancellationToken);
}