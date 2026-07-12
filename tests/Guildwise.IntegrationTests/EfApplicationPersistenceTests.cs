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
    public async Task CreateRaidEvent_Persists_Event_For_Existing_RaidTeam()
    {
        var guild = await AddGuildAsync();
        var raidTeamName = UniqueName("RaidTeam");
        Guid raidTeamId;

        using (var raidTeamContext = _fixture.CreateDbContext())
        {
            var raidTeamResult = await new CreateRaidTeamHandler(new EfGuildRepository(raidTeamContext))
                .HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName));
            raidTeamId = AssertSuccess(raidTeamResult).Id;
        }

        var startTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));

        using (var actContext = _fixture.CreateDbContext())
        {
            var handler = new CreateRaidEventHandler(
                new EfGuildRepository(actContext),
                new EfRaidEventRepository(actContext));

            AssertSuccess(await handler.HandleAsync(new CreateRaidEventCommand(
                guild.Id,
                raidTeamId,
                "Raid Night",
                startTime,
                null,
                "Nerubar Palace",
                RaidDifficulty.Normal,
                null)));
        }

        using var assertContext = _fixture.CreateDbContext();
        var raidEvent = Assert.Single(await new EfRaidEventRepository(assertContext).ListForRaidTeamAsync(raidTeamId));
        Assert.Equal(guild.Id, raidEvent.GuildId);
        Assert.Equal(raidTeamId, raidEvent.RaidTeamId);
        Assert.Equal("Raid Night", raidEvent.Title);
        Assert.Equal(TimeSpan.Zero, raidEvent.StartTime.Offset);
        AssertDateTimeOffsetCloseTo(startTime.ToUniversalTime(), raidEvent.StartTime);
        Assert.Equal("Nerubar Palace", raidEvent.InstanceName);
        Assert.Equal(RaidDifficulty.Normal, raidEvent.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status);
    }

    [Fact]
    public async Task UpdateRaidEvent_Persists_Event_Changes()
    {
        var guild = await AddGuildAsync();
        var raidTeamName = UniqueName("RaidTeam");
        Guid raidTeamId;

        using (var raidTeamContext = _fixture.CreateDbContext())
        {
            var raidTeamResult = await new CreateRaidTeamHandler(new EfGuildRepository(raidTeamContext))
                .HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName));
            raidTeamId = AssertSuccess(raidTeamResult).Id;
        }

        Guid raidEventId;
        using (var createContext = _fixture.CreateDbContext())
        {
            var createHandler = new CreateRaidEventHandler(
                new EfGuildRepository(createContext),
                new EfRaidEventRepository(createContext));

            raidEventId = AssertSuccess(await createHandler.HandleAsync(new CreateRaidEventCommand(
                guild.Id,
                raidTeamId,
                "Raid Night",
                DateTimeOffset.UtcNow.AddDays(1),
                null,
                "Nerubar Palace",
                RaidDifficulty.Normal,
                null))).Id;
        }

        var updatedStartTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));
        using (var updateContext = _fixture.CreateDbContext())
        {
            var updateHandler = new UpdateRaidEventHandler(
                new EfGuildRepository(updateContext),
                new EfRaidEventRepository(updateContext));

            AssertSuccess(await updateHandler.HandleAsync(new UpdateRaidEventCommand(
                raidEventId,
                guild.Id,
                raidTeamId,
                "Updated Raid",
                updatedStartTime,
                null,
                "Manaforge Omega",
                RaidDifficulty.Heroic,
                "Updated notes")));
        }

        using var assertContext = _fixture.CreateDbContext();
        var raidEvent = await new EfRaidEventRepository(assertContext).GetByIdAsync(raidEventId);
        Assert.NotNull(raidEvent);
        Assert.Equal("Updated Raid", raidEvent.Title);
        Assert.Equal("Manaforge Omega", raidEvent.InstanceName);
        Assert.Equal(RaidDifficulty.Heroic, raidEvent.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status);
        Assert.Equal("Updated notes", raidEvent.Notes);
        Assert.Equal(TimeSpan.Zero, raidEvent.StartTime.Offset);
        AssertDateTimeOffsetCloseTo(updatedStartTime.ToUniversalTime(), raidEvent.StartTime);
    }

    [Fact]
    public async Task CancelRaidEvent_Persists_Cancelled_Status()
    {
        var guild = await AddGuildAsync();
        Guid raidTeamId;

        using (var raidTeamContext = _fixture.CreateDbContext())
        {
            var raidTeamResult = await new CreateRaidTeamHandler(new EfGuildRepository(raidTeamContext))
                .HandleAsync(new CreateRaidTeamCommand(guild.Id, UniqueName("RaidTeam")));
            raidTeamId = AssertSuccess(raidTeamResult).Id;
        }

        Guid raidEventId;
        using (var createContext = _fixture.CreateDbContext())
        {
            var createHandler = new CreateRaidEventHandler(
                new EfGuildRepository(createContext),
                new EfRaidEventRepository(createContext));

            raidEventId = AssertSuccess(await createHandler.HandleAsync(new CreateRaidEventCommand(
                guild.Id,
                raidTeamId,
                "Raid Night",
                DateTimeOffset.UtcNow.AddDays(1),
                null,
                "Nerubar Palace",
                RaidDifficulty.Normal,
                null))).Id;
        }

        using (var cancelContext = _fixture.CreateDbContext())
        {
            var cancelHandler = new CancelRaidEventHandler(new EfRaidEventRepository(cancelContext));

            var cancelled = AssertSuccess(await cancelHandler.HandleAsync(new CancelRaidEventCommand(raidEventId)));
            Assert.Equal(RaidEventStatus.Cancelled, cancelled.Status);
        }

        using var assertContext = _fixture.CreateDbContext();
        var raidEvent = await new EfRaidEventRepository(assertContext).GetByIdAsync(raidEventId);
        Assert.NotNull(raidEvent);
        Assert.Equal(RaidEventStatus.Cancelled, raidEvent.Status);
    }

    [Fact]
    public async Task SetAndListRaidEventSignup_Persists_Signup_For_GuildMember()
    {
        var player = await AddPlayerWithMainCharacterAsync();
        var guild = await AddGuildWithMemberAndRaidTeamAsync(player);
        var raidTeam = guild.RaidTeams.Single();
        Guid raidEventId;

        using (var createEventContext = _fixture.CreateDbContext())
        {
            var createHandler = new CreateRaidEventHandler(
                new EfGuildRepository(createEventContext),
                new EfRaidEventRepository(createEventContext));

            raidEventId = AssertSuccess(await createHandler.HandleAsync(new CreateRaidEventCommand(
                guild.Id,
                raidTeam.Id,
                "Raid Night",
                DateTimeOffset.UtcNow.AddDays(1),
                null,
                "Nerubar Palace",
                RaidDifficulty.Normal,
                null))).Id;
        }

        using (var signupContext = _fixture.CreateDbContext())
        {
            var handler = new SetRaidEventSignupHandler(
                new EfGuildRepository(signupContext),
                new EfPlayerRepository(signupContext),
                new EfRaidEventRepository(signupContext));

            var signup = AssertSuccess(await handler.HandleAsync(new SetRaidEventSignupCommand(
                raidEventId,
                player.Id,
                RaidEventSignupStatus.Signed)));

            Assert.Equal(RaidEventSignupStatus.Signed, signup.Status);
            Assert.Equal(player.Id, signup.PlayerId);
            Assert.True(signup.HasMainCharacter);
        }

        using (var updateContext = _fixture.CreateDbContext())
        {
            var handler = new SetRaidEventSignupHandler(
                new EfGuildRepository(updateContext),
                new EfPlayerRepository(updateContext),
                new EfRaidEventRepository(updateContext));

            var updated = AssertSuccess(await handler.HandleAsync(new SetRaidEventSignupCommand(
                raidEventId,
                player.Id,
                RaidEventSignupStatus.Declined)));

            Assert.Equal(RaidEventSignupStatus.Declined, updated.Status);
        }

        using var assertContext = _fixture.CreateDbContext();
        var listHandler = new ListRaidEventSignupsHandler(
            new EfGuildRepository(assertContext),
            new EfPlayerRepository(assertContext),
            new EfRaidEventRepository(assertContext));

        var signups = await listHandler.HandleAsync(new ListRaidEventSignupsQuery(raidEventId));
        var listed = Assert.Single(signups);
        Assert.Equal(player.Id, listed.PlayerId);
        Assert.Equal(RaidEventSignupStatus.Declined, listed.Status);
        Assert.True(listed.HasMainCharacter);
        Assert.Equal(GuildRank.Member, listed.GuildRank);
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

    private static T AssertSuccess<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Null(result.Failure);
        return result.Value;
    }

    private static void AssertDateTimeOffsetCloseTo(DateTimeOffset expected, DateTimeOffset actual)
        => Assert.InRange((actual - expected).Duration(), TimeSpan.Zero, TimeSpan.FromMilliseconds(1));

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
