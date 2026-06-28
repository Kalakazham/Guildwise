using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.Common.Results;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.Players.DeletePlayer;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Domain;
using Guildwise.Infrastructure.Persistence;
using Guildwise.IntegrationTests.Persistence;

namespace Guildwise.IntegrationTests;

[Collection(PostgreSqlTestCollection.Name)]
public sealed class EfApplicationPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlTestFixture _fixture;

    public EfApplicationPersistenceTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    public Task InitializeAsync()
        => _fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;

    [Fact]
    public async Task CreateCharacter_Persists_Character_For_Existing_Player()
    {
        var player = await AddPlayerAsync();
        var characterName = UniqueName("Alysa");

        using (var actContext = _fixture.CreateDbContext())
        {
            var handler = new CreateCharacterHandler(new EfPlayerRepository(actContext));

            AssertSuccess(await handler.HandleAsync(new CreateCharacterCommand(
                player.Id,
                characterName,
                "EU",
                "Draenor",
                CharacterClass.Mage,
                CharacterSpecialization.MageFrost,
                CharacterRole.Damage)));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);
        var character = Assert.Single(loaded!.Characters);
        Assert.Equal(characterName, character.Name);
    }

    [Fact]
    public async Task SetMainCharacter_Persists_Main_Character_Assignment()
    {
        var player = await AddPlayerWithCharacterAsync();
        var character = player.Characters.Single();

        using (var actContext = _fixture.CreateDbContext())
        {
            var handler = new SetMainCharacterHandler(new EfPlayerRepository(actContext));

            AssertSuccess(await handler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id)));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);
        Assert.Equal(character.Id, loaded!.MainCharacterId);
    }

    [Fact]
    public async Task CreateRaidTeam_Persists_Raid_Team_For_Existing_Guild()
    {
        var guild = await AddGuildAsync();
        var raidTeamName = UniqueName("RaidTeam");

        using (var actContext = _fixture.CreateDbContext())
        {
            var handler = new CreateRaidTeamHandler(new EfGuildRepository(actContext));

            AssertSuccess(await handler.HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName)));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);
        var raidTeam = Assert.Single(loaded!.RaidTeams);
        Assert.Equal(raidTeamName, raidTeam.Name);
    }

    [Fact]
    public async Task AddPlayerToGuild_Persists_Guild_Membership()
    {
        var guild = await AddGuildAsync();
        var player = await AddPlayerAsync();

        using (var actContext = _fixture.CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToGuildHandler(guildRepository, playerRepository);

            AssertSuccess(await handler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);
        var member = Assert.Single(loaded!.Members);
        Assert.Equal(player.Id, member.PlayerId);
    }

    [Fact]
    public async Task AddPlayerToRaidTeam_Persists_Raid_Team_Membership()
    {
        var player = await AddPlayerWithMainCharacterAsync();
        var guild = await AddGuildWithMemberAndRaidTeamAsync(player);
        var raidTeam = guild.RaidTeams.Single();

        using (var actContext = _fixture.CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToRaidTeamHandler(guildRepository, playerRepository);

            AssertSuccess(await handler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);
        var loadedRaidTeam = Assert.Single(loaded!.RaidTeams);
        var raidTeamMember = Assert.Single(loadedRaidTeam.Members);
        Assert.Equal(player.Id, raidTeamMember.PlayerId);
    }

    [Fact]
    public async Task DeletePlayer_Rolls_Back_Guild_Changes_When_Player_Removal_Throws()
    {
        var player = await AddPlayerWithMainCharacterAsync();
        var guild = await AddGuildWithMemberAndRaidTeamMemberAsync(player);

        using (var arrangeAssertContext = _fixture.CreateDbContext())
        {
            var arrangedGuild = await new EfGuildRepository(arrangeAssertContext).GetByIdAsync(guild.Id);
            Assert.NotNull(arrangedGuild);
            Assert.Equal(player.Id, Assert.Single(arrangedGuild.Members).PlayerId);
            Assert.Equal(player.Id, Assert.Single(Assert.Single(arrangedGuild.RaidTeams).Members).PlayerId);
        }

        using (var actContext = _fixture.CreateDbContext())
        {
            var playerRepository = new EfPlayerRepository(actContext);
            var throwingPlayerRepository = new ThrowingRemovePlayerRepository(
                playerRepository,
                () => actContext.Database.CurrentTransaction is not null);
            var handler = new DeletePlayerHandler(
                new EfGuildRepository(actContext),
                throwingPlayerRepository,
                new EfTransactionRunner(actContext));

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.HandleAsync(new DeletePlayerCommand(player.Id)));

            Assert.Equal("Forced player removal failure.", exception.Message);
            Assert.True(throwingPlayerRepository.SawActiveTransaction);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loadedPlayer = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);
        var loadedGuild = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);

        Assert.NotNull(loadedPlayer);
        Assert.NotNull(loadedGuild);
        Assert.Equal(player.Id, Assert.Single(loadedGuild.Members).PlayerId);
        Assert.Equal(player.Id, Assert.Single(Assert.Single(loadedGuild.RaidTeams).Members).PlayerId);
    }

    private async Task<Player> AddPlayerAsync()
    {
        var player = Player.Create(UniqueName("Player"));

        using var context = _fixture.CreateDbContext();
        await new EfPlayerRepository(context).AddAsync(player);

        return player;
    }

    private async Task<Player> AddPlayerWithCharacterAsync()
    {
        var player = Player.Create(UniqueName("Player"));
        player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);

        using var context = _fixture.CreateDbContext();
        await new EfPlayerRepository(context).AddAsync(player);

        return player;
    }

    private async Task<Player> AddPlayerWithMainCharacterAsync()
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

        using var context = _fixture.CreateDbContext();
        await new EfPlayerRepository(context).AddAsync(player);

        return player;
    }

    private async Task<Guild> AddGuildAsync()
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");

        using var context = _fixture.CreateDbContext();
        await new EfGuildRepository(context).AddAsync(guild);

        return guild;
    }

    private async Task<Guild> AddGuildWithMemberAndRaidTeamAsync(Player player)
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        guild.AddMember(player, GuildRank.Member);
        guild.CreateRaidTeam(UniqueName("RaidTeam"));

        using var context = _fixture.CreateDbContext();
        await new EfGuildRepository(context).AddAsync(guild);

        return guild;
    }

    private async Task<Guild> AddGuildWithMemberAndRaidTeamMemberAsync(Player player)
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        guild.AddMember(player, GuildRank.Member);
        var raidTeam = guild.CreateRaidTeam(UniqueName("RaidTeam"));
        guild.AddPlayerToRaidTeam(raidTeam, player);

        using var context = _fixture.CreateDbContext();
        await new EfGuildRepository(context).AddAsync(guild);

        return guild;
    }

    private static string UniqueName(string prefix)
        => $"{prefix}{Guid.NewGuid():N}";

    private static void AssertSuccess<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Failure);
    }

    private sealed class ThrowingRemovePlayerRepository : IPlayerRepository
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
