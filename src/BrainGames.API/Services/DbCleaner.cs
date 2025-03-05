using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace BrainGames.API.Services;

public interface IDbCleaner
{
    Task CleanAsync(CancellationToken cancellationToken = default);
}

public sealed class DbCleaner(IConnectionMultiplexer connectionMultiplexer) : IDbCleaner
{
    public async Task CleanAsync(CancellationToken cancellationToken)
    {
        var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
        await server.FlushDatabaseAsync();
    }
}