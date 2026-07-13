using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.Common.Results;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.Players.DeletePlayer;
using Guildwise.Application.RaidEvents.CancelRaidEvent;
using Guildwise.Application.RaidEvents.CreateRaidEvent;
using Guildwise.Application.RaidEvents.ListRaidEventSignups;
using Guildwise.Application.RaidEvents.SetRaidEventSignup;
using Guildwise.Application.RaidEvents.UpdateRaidEvent;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Domain;
using Guildwise.Infrastructure.Persistence;
using Guildwise.IntegrationTests.Persistence;
namespace Guildwise.IntegrationTests;

public abstract class EfApplicationPersistenceTestBase : IAsyncLifetime
{
    protected EfApplicationPersistenceTestBase(PostgreSqlTestFixture fixture)
    {
        Fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }
    protected PostgreSqlTestFixture Fixture { get; }
    public Task InitializeAsync()
        => Fixture.ResetDatabaseAsync();
    public Task DisposeAsync()
        => Task.CompletedTask;
    protected async Task<Player> AddPlayerAsync()
    {
        var player = Player.Create(UniqueName("Player"));

        using var context = Fixture.CreateDbContext();
        await new EfPlayerRepository(context).AddAsync(player);

        return player;
    }

    protected async Task<Player> AddPlayerWithCharacterAsync()
    {
        var player = Player.Create(UniqueName("Player"));
        player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

        using var context = Fixture.CreateDbContext();
        await new EfPlayerRepository(context).AddAsync(player);

        return player;
    }

    protected async Task<Player> AddPlayerWithMainCharacterAsync()
    {
        var player = Player.Create(UniqueName("Player"));
        var character = player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Shaman,
            CharacterSpecialization.ShamanRestoration,
            CharacterRole.Healer);
        player.SetMainCharacter(character);

        using var context = Fixture.CreateDbContext();
        await new EfPlayerRepository(context).AddAsync(player);

        return player;
    }

    protected async Task<Guild> AddGuildAsync()
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");

        using var context = Fixture.CreateDbContext();
        await new EfGuildRepository(context).AddAsync(guild);

        return guild;
    }

    protected async Task<Guild> AddGuildWithMemberAndRaidTeamAsync(Player player)
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        guild.AddMember(player, GuildRank.Member);
        guild.CreateRaidTeam(UniqueName("RaidTeam"));

        using var context = Fixture.CreateDbContext();
        await new EfGuildRepository(context).AddAsync(guild);

        return guild;
    }

    protected async Task<Guild> AddGuildWithMemberAndRaidTeamMemberAsync(Player player)
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        guild.AddMember(player, GuildRank.Member);
        var raidTeam = guild.CreateRaidTeam(UniqueName("RaidTeam"));
        guild.AddPlayerToRaidTeam(raidTeam, player);

        using var context = Fixture.CreateDbContext();
        await new EfGuildRepository(context).AddAsync(guild);

        return guild;
    }

    protected static string UniqueName(string prefix)
        => $"{prefix}{Guid.NewGuid():N}";

    protected static T AssertSuccess<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Failure);
        return result.Value;
    }

    protected static void AssertDateTimeOffsetCloseTo(DateTimeOffset expected, DateTimeOffset actual)
        => Assert.InRange((actual - expected).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

    protected sealed class ThrowingRemovePlayerRepository : IPlayerRepository
    {
        private readonly IPlayerRepository _inner;

        private readonly Func<bool> _hasActiveTransaction;

        public ThrowingRemovePlayerRepository(IPlayerRepository inner, Func<bool> hasActiveTransaction)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _hasActiveTransaction = hasActiveTransaction ?? throw new ArgumentNullException(nameof(hasActiveTransaction));
        }

        public bool SawActiveTransaction { get; private set; }

        public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => _inner.GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyCollection<Player>> ListAsync(CancellationToken cancellationToken = default)
            => _inner.ListAsync(cancellationToken);

        public Task AddAsync(Player player, CancellationToken cancellationToken = default)
            => _inner.AddAsync(player, cancellationToken);

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            SawActiveTransaction = _hasActiveTransaction();
            throw new InvalidOperationException("Forced player removal failure.");
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => _inner.SaveChangesAsync(cancellationToken);
    }
}
