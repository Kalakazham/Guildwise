using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;

namespace Guildwise.Infrastructure.Persistence;

public sealed class GuildwiseDbContext : DbContext
{
    public GuildwiseDbContext(DbContextOptions<GuildwiseDbContext> options)
        : base(options)
    {
    }

    public DbSet<Guild> Guilds => Set<Guild>();

    public DbSet<Player> Players => Set<Player>();

    public DbSet<RaidEvent> RaidEvents => Set<RaidEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GuildwiseDbContext).Assembly);
    }
}
