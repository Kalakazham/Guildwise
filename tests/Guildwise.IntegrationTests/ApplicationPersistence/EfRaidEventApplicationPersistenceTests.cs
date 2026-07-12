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
public sealed class EfRaidEventApplicationPersistenceTests : EfApplicationPersistenceTestBase
{
    public EfRaidEventApplicationPersistenceTests(PostgreSqlTestFixture fixture)
        : base(fixture)
    {
    }
    [Fact]
    public async Task CreateRaidEvent_Persists_Event_For_Existing_RaidTeam()
    {
        var guild = await AddGuildAsync();
        var raidTeamName = UniqueName("RaidTeam");
        Guid raidTeamId;

        using (var raidTeamContext = Fixture.CreateDbContext())
        {
            var raidTeamResult = await new CreateRaidTeamHandler(new EfGuildRepository(raidTeamContext))
                .HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName));
            raidTeamId = AssertSuccess(raidTeamResult).Id;
        }

        var startTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));

        using (var actContext = Fixture.CreateDbContext())
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

        using var assertContext = Fixture.CreateDbContext();
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

        using (var raidTeamContext = Fixture.CreateDbContext())
        {
            var raidTeamResult = await new CreateRaidTeamHandler(new EfGuildRepository(raidTeamContext))
                .HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName));
            raidTeamId = AssertSuccess(raidTeamResult).Id;
        }

        Guid raidEventId;
        using (var createContext = Fixture.CreateDbContext())
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
        using (var updateContext = Fixture.CreateDbContext())
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

        using var assertContext = Fixture.CreateDbContext();
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

        using (var raidTeamContext = Fixture.CreateDbContext())
        {
            var raidTeamResult = await new CreateRaidTeamHandler(new EfGuildRepository(raidTeamContext))
                .HandleAsync(new CreateRaidTeamCommand(guild.Id, UniqueName("RaidTeam")));
            raidTeamId = AssertSuccess(raidTeamResult).Id;
        }

        Guid raidEventId;
        using (var createContext = Fixture.CreateDbContext())
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

        using (var cancelContext = Fixture.CreateDbContext())
        {
            var cancelHandler = new CancelRaidEventHandler(new EfRaidEventRepository(cancelContext));

            var cancelled = AssertSuccess(await cancelHandler.HandleAsync(new CancelRaidEventCommand(raidEventId)));
            Assert.Equal(RaidEventStatus.Cancelled, cancelled.Status);
        }

        using var assertContext = Fixture.CreateDbContext();
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

        using (var createEventContext = Fixture.CreateDbContext())
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

        using (var signupContext = Fixture.CreateDbContext())
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

        using (var updateContext = Fixture.CreateDbContext())
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

        using var assertContext = Fixture.CreateDbContext();
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
}
