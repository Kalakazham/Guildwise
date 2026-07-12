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
public sealed class EfGuildRaidTeamApplicationPersistenceTests : EfApplicationPersistenceTestBase
{
    public EfGuildRaidTeamApplicationPersistenceTests(PostgreSqlTestFixture fixture)
        : base(fixture)
    {
    }
    [Fact]
    public async Task CreateRaidTeam_Persists_Raid_Team_For_Existing_Guild()
    {
        var guild = await AddGuildAsync();
        var raidTeamName = UniqueName("RaidTeam");

        using (var actContext = Fixture.CreateDbContext())
        {
            var handler = new CreateRaidTeamHandler(new EfGuildRepository(actContext));

            AssertSuccess(await handler.HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName)));
        }

        using var assertContext = Fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);
        var raidTeam = Assert.Single(loaded!.RaidTeams);
        Assert.Equal(raidTeamName, raidTeam.Name);
    }

    [Fact]
    public async Task AddPlayerToGuild_Persists_Guild_Membership()
    {
        var guild = await AddGuildAsync();
        var player = await AddPlayerAsync();

        using (var actContext = Fixture.CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToGuildHandler(guildRepository, playerRepository);

            AssertSuccess(await handler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member)));
        }

        using var assertContext = Fixture.CreateDbContext();
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

        using (var actContext = Fixture.CreateDbContext())
        {
            var guildRepository = new EfGuildRepository(actContext);
            var playerRepository = new EfPlayerRepository(actContext);
            var handler = new AddPlayerToRaidTeamHandler(guildRepository, playerRepository);

            AssertSuccess(await handler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));
        }

        using var assertContext = Fixture.CreateDbContext();
        var loaded = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);
        var loadedRaidTeam = Assert.Single(loaded!.RaidTeams);
        var raidTeamMember = Assert.Single(loadedRaidTeam.Members);
        Assert.Equal(player.Id, raidTeamMember.PlayerId);
    }
}
