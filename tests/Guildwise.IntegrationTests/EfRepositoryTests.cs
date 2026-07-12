using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;
using Guildwise.Infrastructure;
using Guildwise.Infrastructure.Persistence;
using Guildwise.IntegrationTests.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.IntegrationTests;

[Collection(PostgreSqlTestCollection.Name)]
public sealed class EfRepositoryTests : IAsyncLifetime
{
    private readonly PostgreSqlTestFixture _fixture;

    public EfRepositoryTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    public Task InitializeAsync()
        => _fixture.ResetDatabaseAsync();

    public Task DisposeAsync()
        => Task.CompletedTask;

    [Fact]
    public void AddPostgresInfrastructure_Registers_Ef_Aggregate_Root_Repositories()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:GuildwiseDatabase"] = _fixture.ConnectionString
            })
            .Build();
        var services = new ServiceCollection();

        services.AddPostgresInfrastructure(configuration);

        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IGuildRepository)
                && descriptor.ImplementationType == typeof(EfGuildRepository)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IPlayerRepository)
                && descriptor.ImplementationType == typeof(EfPlayerRepository)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(IRaidEventRepository)
                && descriptor.ImplementationType == typeof(EfRaidEventRepository)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
        Assert.Contains(
            services,
            descriptor => descriptor.ServiceType == typeof(ITransactionRunner)
                && descriptor.ImplementationType == typeof(EfTransactionRunner)
                && descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public async Task EfTransactionRunner_Rolls_Back_When_Operation_Throws()
    {
        var guildName = UniqueName("Guild");

        using (var actContext = _fixture.CreateDbContext())
        {
            var transactionRunner = new EfTransactionRunner(actContext);
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                transactionRunner.ExecuteAsync(async cancellationToken =>
                {
                    await actContext.Guilds.AddAsync(Guild.Create(guildName, "EU", "Draenor"), cancellationToken);
                    await actContext.SaveChangesAsync(cancellationToken);
                    throw new InvalidOperationException("Forced transaction failure.");
                }));

            Assert.Equal("Forced transaction failure.", exception.Message);
        }

        using var assertContext = _fixture.CreateDbContext();
        Assert.Empty(assertContext.Guilds.Where(guild => guild.Name == guildName));
    }

    [Fact]
    public async Task EfPlayerRepository_Saves_And_Loads_Player()
    {
        var displayName = UniqueName("Player");
        var player = Player.Create(displayName);

        using (var arrangeContext = _fixture.CreateDbContext())
        {
            await new EfPlayerRepository(arrangeContext).AddAsync(player);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);

        Assert.NotNull(loaded);
        Assert.Equal(displayName, loaded.DisplayName);
        Assert.Null(loaded.MainCharacterId);
        Assert.Empty(loaded.Characters);
    }

    [Fact]
    public async Task EfPlayerRepository_Saves_And_Loads_Player_With_Characters_And_MainCharacter()
    {
        var player = Player.Create(UniqueName("Player"));
        var character = player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);
        player.SetMainCharacter(character);

        using (var arrangeContext = _fixture.CreateDbContext())
        {
            await new EfPlayerRepository(arrangeContext).AddAsync(player);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);

        Assert.NotNull(loaded);
        Assert.Equal(character.Id, loaded.MainCharacterId);
        var loadedCharacter = Assert.Single(loaded.Characters);
        Assert.Equal(character.Id, loadedCharacter.Id);
        Assert.Equal(CharacterClass.Paladin, loadedCharacter.CharacterClass);
        Assert.Equal(CharacterSpecialization.PaladinRetribution, loadedCharacter.Specialization);
        Assert.Equal(CharacterRole.Damage, loadedCharacter.Role);
    }

    [Fact]
    public async Task EfPlayerRepository_Rolls_Back_Player_With_MainCharacter_When_Second_Save_Fails()
    {
        var player = Player.Create(UniqueName("Player"));
        var character = player.AddCharacter(
            UniqueName("Alysa"),
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage);
        player.SetMainCharacter(character);
        var interceptor = new ThrowOnSecondSaveChangesInterceptor();
        var options = new DbContextOptionsBuilder<GuildwiseDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .AddInterceptors(interceptor)
            .Options;

        using (var arrangeContext = new GuildwiseDbContext(options))
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => new EfPlayerRepository(arrangeContext).AddAsync(player));

            Assert.Equal("Forced second save failure.", exception.Message);
        }

        using var assertContext = _fixture.CreateDbContext();
        Assert.Null(await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id));
        Assert.False(await assertContext.Set<Character>().AnyAsync(existingCharacter => existingCharacter.Id == character.Id));
    }

    [Fact]
    public async Task EfGuildRepository_Saves_And_Loads_Guild()
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");

        using (var arrangeContext = _fixture.CreateDbContext())
        {
            await new EfGuildRepository(arrangeContext).AddAsync(guild);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);

        Assert.NotNull(loaded);
        Assert.Equal(guild.Name, loaded.Name);
        Assert.Equal("EU", loaded.Region);
        Assert.Equal("Draenor", loaded.Realm);
        Assert.Empty(loaded.Members);
        Assert.Empty(loaded.RaidTeams);
    }

    [Fact]
    public async Task EfGuildRepository_Saves_And_Loads_Guild_Members_RaidTeams_And_RaidTeamMembers()
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
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        var guildMember = guild.AddMember(player, GuildRank.Officer);
        guildMember.AddAdditionalRole(AdditionalGuildRole.RaidLead);
        guildMember.AddAdditionalRole(AdditionalGuildRole.Recruiter);
        var raidTeam = guild.CreateRaidTeam(UniqueName("RaidTeam"));
        guild.AddPlayerToRaidTeam(raidTeam, player);

        using (var playerContext = _fixture.CreateDbContext())
        {
            await new EfPlayerRepository(playerContext).AddAsync(player);
        }

        using (var guildContext = _fixture.CreateDbContext())
        {
            await new EfGuildRepository(guildContext).AddAsync(guild);
        }

        using var assertContext = _fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);

        Assert.NotNull(loaded);
        var loadedMember = Assert.Single(loaded.Members);
        Assert.Equal(player.Id, loadedMember.PlayerId);
        Assert.Equal(GuildRank.Officer, loadedMember.Rank);
        Assert.Equal(
            [AdditionalGuildRole.RaidLead, AdditionalGuildRole.Recruiter],
            loadedMember.AdditionalRoles.OrderBy(role => role).ToArray());

        var loadedRaidTeam = Assert.Single(loaded.RaidTeams);
        Assert.Equal(raidTeam.Name, loadedRaidTeam.Name);
        var loadedRaidTeamMember = Assert.Single(loadedRaidTeam.Members);
        Assert.Equal(player.Id, loadedRaidTeamMember.PlayerId);
    }

    [Fact]
    public async Task EfRaidEventRepository_Saves_Loads_And_Lists_RaidEvent()
    {
        var guild = Guild.Create(UniqueName("Guild"), "EU", "Draenor");
        var raidTeam = guild.CreateRaidTeam(UniqueName("RaidTeam"));
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);
        var raidEvent = RaidEvent.Create(
            guild.Id,
            raidTeam.Id,
            UniqueName("RaidEvent"),
            startTime,
            endTime,
            "Nerubar Palace",
            RaidDifficulty.Heroic,
            "Bring flasks.");

        using (var guildContext = _fixture.CreateDbContext())
        {
            await new EfGuildRepository(guildContext).AddAsync(guild);
        }

        using (var arrangeContext = _fixture.CreateDbContext())
        {
            await new EfRaidEventRepository(arrangeContext).AddAsync(raidEvent);
        }

        using var assertContext = _fixture.CreateDbContext();
        var repository = new EfRaidEventRepository(assertContext);
        var loaded = await repository.GetByIdAsync(raidEvent.Id);
        var guildEvents = await repository.ListForGuildAsync(guild.Id);
        var raidTeamEvents = await repository.ListForRaidTeamAsync(raidTeam.Id);

        Assert.NotNull(loaded);
        Assert.Equal(raidEvent.Id, loaded.Id);
        Assert.Equal(guild.Id, loaded.GuildId);
        Assert.Equal(raidTeam.Id, loaded.RaidTeamId);
        Assert.Equal(raidEvent.Title, loaded.Title);
        AssertDateTimeOffsetCloseTo(startTime, loaded.StartTime);
        Assert.NotNull(loaded.EndTime);
        AssertDateTimeOffsetCloseTo(endTime, loaded.EndTime.Value);
        Assert.Equal("Nerubar Palace", loaded.InstanceName);
        Assert.Equal(RaidDifficulty.Heroic, loaded.Difficulty);
        Assert.Equal("Bring flasks.", loaded.Notes);
        Assert.Equal(raidEvent.Id, Assert.Single(guildEvents).Id);
        Assert.Equal(raidEvent.Id, Assert.Single(raidTeamEvents).Id);
    }

    private static string UniqueName(string prefix)
        => $"{prefix}{Guid.NewGuid():N}";

    private static void AssertDateTimeOffsetCloseTo(DateTimeOffset expected, DateTimeOffset actual)
        => Assert.InRange((actual - expected).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

    private sealed class ThrowOnSecondSaveChangesInterceptor : SaveChangesInterceptor
    {
        private int _saveChangesCalls;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            _saveChangesCalls++;

            if (_saveChangesCalls == 2)
            {
                throw new InvalidOperationException("Forced second save failure.");
            }

            return new ValueTask<InterceptionResult<int>>(result);
        }
    }
}
