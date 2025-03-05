using BrainGames.API.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace BrainGames.API.Services;

public interface IDbCleaner
{
    Task CleanAsync(CancellationToken cancellationToken = default);
    Task Seed(CancellationToken cancellationToken = default);
}

public sealed class DbCleaner(IConnectionMultiplexer connectionMultiplexer, BrainGamesDbContext dbContext) : IDbCleaner
{
    public async Task CleanAsync(CancellationToken cancellationToken)
    {
        var server = connectionMultiplexer.GetServer(connectionMultiplexer.GetEndPoints().First());
        await server.FlushDatabaseAsync();
    }
    
    public async Task Seed(CancellationToken cancellationToken)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

}