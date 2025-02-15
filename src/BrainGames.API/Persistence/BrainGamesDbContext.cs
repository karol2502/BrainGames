using BrainGames.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace BrainGames.API.Persistence;

public sealed class BrainGamesDbContext(DbContextOptions<BrainGamesDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrainGamesDbContext).Assembly);
    }
} 