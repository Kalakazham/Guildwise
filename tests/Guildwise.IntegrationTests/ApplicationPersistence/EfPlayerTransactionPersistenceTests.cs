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
public sealed class EfPlayerTransactionPersistenceTests : EfApplicationPersistenceTestBase
{
    public EfPlayerTransactionPersistenceTests(PostgreSqlTestFixture fixture)
        : base(fixture)
    {
    }
    [Fact]
    public async Task DeletePlayer_Rolls_Back_Guild_Changes_When_Player_Removal_Throws()
    {
        var player = await AddPlayerWithMainCharacterAsync();
        var guild = await AddGuildWithMemberAndRaidTeamMemberAsync(player);

        using (var arrangeAssertContext = Fixture.CreateDbContext())
        {
            var arrangedGuild = await new EfGuildRepository(arrangeAssertContext).GetByIdAsync(guild.Id);
            Assert.NotNull(arrangedGuild);
            Assert.Equal(player.Id, Assert.Single(arrangedGuild.Members).PlayerId);
            Assert.Equal(player.Id, Assert.Single(Assert.Single(arrangedGuild.RaidTeams).Members).PlayerId);
        }

        using (var actContext = Fixture.CreateDbContext())
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

        using var assertContext = Fixture.CreateDbContext();
        var loadedPlayer = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);
        var loadedGuild = await new EfGuildRepository(assertContext).GetByIdAsync(guild.Id);

        Assert.NotNull(loadedPlayer);
        Assert.NotNull(loadedGuild);
        Assert.Equal(player.Id, Assert.Single(loadedGuild.Members).PlayerId);
        Assert.Equal(player.Id, Assert.Single(Assert.Single(loadedGuild.RaidTeams).Members).PlayerId);
    }
}
