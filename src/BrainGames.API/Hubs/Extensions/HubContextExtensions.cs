using Microsoft.AspNetCore.SignalR;

namespace BrainGames.API.Hubs.Extensions;

public static class HubContextExtensions
{
    public static Task SendGameCommandToAllAsync<T>(this IHubContext<GameHub> hub, string lobbyId, string command, T payload, CancellationToken cancellationToken = default)
    {
        return hub.Clients.Group(lobbyId).SendAsync("HandleGameCommand", command, payload, cancellationToken: cancellationToken);
    }
    
    public static Task SendGameCommandToPlayerAsync<T>(this IHubContext<GameHub> hub, string command, T payload, string connectionId, CancellationToken cancellationToken = default)
    {
        return hub.Clients.Client(connectionId).SendAsync("HandleGameCommand", command, payload, cancellationToken: cancellationToken);
    }
}
