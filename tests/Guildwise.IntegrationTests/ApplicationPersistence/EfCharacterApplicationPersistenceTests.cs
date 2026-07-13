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
public sealed class EfCharacterApplicationPersistenceTests : EfApplicationPersistenceTestBase
{
    public EfCharacterApplicationPersistenceTests(PostgreSqlTestFixture fixture)
        : base(fixture)
    {
    }
    [Fact]
    public async Task CreateCharacter_Persists_Character_For_Existing_Player()
    {
        var player = await AddPlayerAsync();
        var characterName = UniqueName("Alysa");

        using (var actContext = Fixture.CreateDbContext())
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

        using var assertContext = Fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);
        var character = Assert.Single(loaded!.Characters);
        Assert.Equal(characterName, character.Name);
    }

    [Fact]
    public async Task SetMainCharacter_Persists_Main_Character_Assignment()
    {
        var player = await AddPlayerWithCharacterAsync();
        var character = player.Characters.Single();

        using (var actContext = Fixture.CreateDbContext())
        {
            var handler = new SetMainCharacterHandler(new EfPlayerRepository(actContext));

            AssertSuccess(await handler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id)));
        }

        using var assertContext = Fixture.CreateDbContext();
        var loaded = await new EfPlayerRepository(assertContext).GetByIdAsync(player.Id);
        Assert.Equal(character.Id, loaded!.MainCharacterId);
    }
}
