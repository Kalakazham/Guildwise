using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.DeleteCharacter;
using Guildwise.Application.Characters.GetCharacter;
using Guildwise.Application.Characters.ListCharacters;
using Guildwise.Application.Characters.ListCharactersForPlayer;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.Characters.UpdateCharacter;
using Guildwise.Application.Common;
using Guildwise.Application.Contracts.Characters;
using Guildwise.Application.Contracts.GuildMembers;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Application.Contracts.Players;
using Guildwise.Application.Contracts.RaidTeams;
using Guildwise.Application.GuildMembers.AddAdditionalRoleToGuildMember;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.GuildMembers.RemoveAdditionalRoleFromGuildMember;
using Guildwise.Application.Guilds.CreateGuild;
using Guildwise.Application.Guilds.DeleteGuild;
using Guildwise.Application.Guilds.GetGuild;
using Guildwise.Application.Guilds.ListGuilds;
using Guildwise.Application.Guilds.UpdateGuild;
using Guildwise.Application.Players.CreatePlayer;
using Guildwise.Application.Players.DeletePlayer;
using Guildwise.Application.Players.GetPlayer;
using Guildwise.Application.Players.ListPlayers;
using Guildwise.Application.Players.UpdatePlayer;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Application.RaidTeams.DeleteRaidTeam;
using Guildwise.Application.RaidTeams.GetRaidTeam;
using Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;
using Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;
using Guildwise.Application.RaidTeams.UpdateRaidTeam;
using Guildwise.Domain;

namespace Guildwise.UnitTests;

public sealed class ApplicationUseCaseTests
{
    [Fact]
    public void CreateGuild_Stores_Guild_And_Returns_Dto()
    {
        var context = new TestContext();

        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));

        Assert.Equal("Guildwise", guild.Name);
        Assert.Single(context.GuildRepository.List());
        Assert.Equal(guild.Id, context.GetGuildHandler.Handle(new GetGuildQuery(guild.Id))?.Id);
    }

    [Fact]
    public void UpdateUnknownPlayer_Throws_NotFoundException()
    {
        var context = new TestContext();

        var exception = Assert.Throws<NotFoundException>(() =>
            context.UpdatePlayerHandler.Handle(new UpdatePlayerCommand(Guid.NewGuid(), "Myrmi")));

        Assert.Contains("Player", exception.Message);
    }

    [Fact]
    public void CreateCharacterForUnknownPlayer_Throws_NotFoundException()
    {
        var context = new TestContext();

        Assert.Throws<NotFoundException>(() => context.CreateCharacterHandler.Handle(new CreateCharacterCommand(
            Guid.NewGuid(),
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage)));
    }

    [Fact]
    public void CreateRaidTeamForUnknownGuild_Throws_NotFoundException()
    {
        var context = new TestContext();

        Assert.Throws<NotFoundException>(() => context.CreateRaidTeamHandler.Handle(new CreateRaidTeamCommand(
            Guid.NewGuid(),
            "Team One")));
    }

    [Fact]
    public void AddPlayerToRaidTeam_When_Player_Is_Not_GuildMember_Fails()
    {
        var context = new TestContext();
        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));
        var character = context.CreateCharacterHandler.Handle(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));
        context.SetMainCharacterHandler.Handle(new SetMainCharacterCommand(player.Id, character.Id));
        var raidTeam = context.CreateRaidTeamHandler.Handle(new CreateRaidTeamCommand(guild.Id, "Team One"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            context.AddPlayerToRaidTeamHandler.Handle(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));

        Assert.Contains("guild member", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddPlayerToUnknownRaidTeam_Throws_NotFoundException()
    {
        var context = new TestContext();
        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));
        var character = context.CreateCharacterHandler.Handle(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));
        context.SetMainCharacterHandler.Handle(new SetMainCharacterCommand(player.Id, character.Id));
        context.AddPlayerToGuildHandler.Handle(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));

        Assert.Throws<NotFoundException>(() => context.AddPlayerToRaidTeamHandler.Handle(
            new AddPlayerToRaidTeamCommand(guild.Id, Guid.NewGuid(), player.Id)));
    }

    [Fact]
    public void CreateAndUpdatePlayer_Works_Through_Application_Handler()
    {
        var context = new TestContext();

        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));
        var updated = context.UpdatePlayerHandler.Handle(new UpdatePlayerCommand(player.Id, "Myrmi Two"));

        Assert.Equal("Myrmi Two", updated.DisplayName);
        Assert.Equal("Myrmi Two", context.GetPlayerHandler.Handle(new GetPlayerQuery(player.Id))?.DisplayName);
    }

    [Fact]
    public void CreateCharacter_UpdateCharacter_And_SetMainCharacter_Work()
    {
        var context = new TestContext();
        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));

        var character = context.CreateCharacterHandler.Handle(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        var updated = context.UpdateCharacterHandler.Handle(new UpdateCharacterCommand(
            player.Id,
            character.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFire,
            CharacterRole.Damage));

        var mainCharacter = context.SetMainCharacterHandler.Handle(new SetMainCharacterCommand(player.Id, updated.Id));

        Assert.Equal(CharacterSpecialization.MageFire, updated.Specialization);
        Assert.Equal(updated.Id, mainCharacter.MainCharacterId);
        Assert.Single(context.ListCharactersForPlayerHandler.Handle(new ListCharactersForPlayerQuery(player.Id)));
    }

    [Fact]
    public void CreateRaidTeam_AddPlayerToGuild_And_AddPlayerToRaidTeam_Work()
    {
        var context = new TestContext();
        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));
        var character = context.CreateCharacterHandler.Handle(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));

        context.SetMainCharacterHandler.Handle(new SetMainCharacterCommand(player.Id, character.Id));
        context.AddPlayerToGuildHandler.Handle(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));

        var raidTeam = context.CreateRaidTeamHandler.Handle(new CreateRaidTeamCommand(guild.Id, "Team One"));
        var roster = context.AddPlayerToRaidTeamHandler.Handle(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));

        Assert.Single(roster.Members);
        Assert.Equal(player.Id, roster.Members.Single().PlayerId);
        Assert.Single(context.ListRaidTeamsForGuildHandler.Handle(new ListRaidTeamsForGuildQuery(guild.Id)));
    }

    [Fact]
    public void RenameRaidTeam_And_DeleteRaidTeam_Work_Through_Guild()
    {
        var context = new TestContext();
        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var raidTeam = context.CreateRaidTeamHandler.Handle(new CreateRaidTeamCommand(guild.Id, "Team One"));

        var renamed = context.UpdateRaidTeamHandler.Handle(new UpdateRaidTeamCommand(guild.Id, raidTeam.Id, "Team Two"));
        context.DeleteRaidTeamHandler.Handle(new DeleteRaidTeamCommand(guild.Id, raidTeam.Id));

        Assert.Equal("Team Two", renamed.Name);
        Assert.Empty(context.ListRaidTeamsForGuildHandler.Handle(new ListRaidTeamsForGuildQuery(guild.Id)));
    }

    [Fact]
    public void GuildMember_Roles_Are_Managed_Through_Application_Handler()
    {
        var context = new TestContext();
        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));

        context.AddPlayerToGuildHandler.Handle(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Officer));
        var added = context.AddAdditionalRoleHandler.Handle(new AddAdditionalRoleToGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead));
        var removed = context.RemoveAdditionalRoleHandler.Handle(new RemoveAdditionalRoleFromGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead));

        Assert.Single(added.AdditionalRoles);
        Assert.Empty(removed.AdditionalRoles);
    }

    [Fact]
    public void DeletePlayer_Removes_Guild_Memberships_And_RaidTeam_Memberships()
    {
        var context = new TestContext();
        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));
        var character = context.CreateCharacterHandler.Handle(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));
        context.SetMainCharacterHandler.Handle(new SetMainCharacterCommand(player.Id, character.Id));
        context.AddPlayerToGuildHandler.Handle(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));
        var raidTeam = context.CreateRaidTeamHandler.Handle(new CreateRaidTeamCommand(guild.Id, "Team One"));
        context.AddPlayerToRaidTeamHandler.Handle(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));

        context.DeletePlayerHandler.Handle(new DeletePlayerCommand(player.Id));

        Assert.Null(context.GetPlayerHandler.Handle(new GetPlayerQuery(player.Id)));
        Assert.Empty(context.GetGuildHandler.Handle(new GetGuildQuery(guild.Id))!.Members);
        Assert.Empty(context.GetRaidTeamHandler.Handle(new GetRaidTeamQuery(guild.Id, raidTeam.Id))!.Members);
    }

    [Fact]
    public void DeleteGuild_Removes_Guild()
    {
        var context = new TestContext();
        var guild = context.CreateGuildHandler.Handle(new CreateGuildCommand("Guildwise", "EU", "Draenor"));

        context.DeleteGuildHandler.Handle(new DeleteGuildCommand(guild.Id));

        Assert.Null(context.GetGuildHandler.Handle(new GetGuildQuery(guild.Id)));
        Assert.Empty(context.ListGuildsHandler.Handle(new ListGuildsQuery()));
    }

    [Fact]
    public void CreateCharacter_Returns_Created_Character()
    {
        var context = new TestContext();
        var player = context.CreatePlayerHandler.Handle(new CreatePlayerCommand("Myrmi"));

        var character = context.CreateCharacterHandler.Handle(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.DeathKnight,
            CharacterSpecialization.DeathKnightFrost,
            CharacterRole.Damage));

        Assert.Equal(CharacterClass.DeathKnight, character.CharacterClass);
        Assert.Single(context.ListCharactersHandler.Handle(new ListCharactersQuery()));
    }

    private sealed class TestContext
    {
        public TestContext()
        {
            CreateGuildHandler = new CreateGuildHandler(GuildRepository);
            GetGuildHandler = new GetGuildHandler(GuildRepository);
            ListGuildsHandler = new ListGuildsHandler(GuildRepository);
            UpdateGuildHandler = new UpdateGuildHandler(GuildRepository);
            DeleteGuildHandler = new DeleteGuildHandler(GuildRepository);

            CreatePlayerHandler = new CreatePlayerHandler(PlayerRepository);
            GetPlayerHandler = new GetPlayerHandler(PlayerRepository);
            ListPlayersHandler = new ListPlayersHandler(PlayerRepository);
            UpdatePlayerHandler = new UpdatePlayerHandler(PlayerRepository);
            DeletePlayerHandler = new DeletePlayerHandler(GuildRepository, PlayerRepository);

            CreateCharacterHandler = new CreateCharacterHandler(PlayerRepository);
            GetCharacterHandler = new GetCharacterHandler(PlayerRepository);
            ListCharactersHandler = new ListCharactersHandler(PlayerRepository);
            ListCharactersForPlayerHandler = new ListCharactersForPlayerHandler(PlayerRepository);
            UpdateCharacterHandler = new UpdateCharacterHandler(PlayerRepository);
            DeleteCharacterHandler = new DeleteCharacterHandler(PlayerRepository);
            SetMainCharacterHandler = new SetMainCharacterHandler(PlayerRepository);

            CreateRaidTeamHandler = new CreateRaidTeamHandler(GuildRepository);
            GetRaidTeamHandler = new GetRaidTeamHandler(GuildRepository);
            ListRaidTeamsForGuildHandler = new ListRaidTeamsForGuildHandler(GuildRepository);
            UpdateRaidTeamHandler = new UpdateRaidTeamHandler(GuildRepository);
            DeleteRaidTeamHandler = new DeleteRaidTeamHandler(GuildRepository);
            AddPlayerToRaidTeamHandler = new AddPlayerToRaidTeamHandler(GuildRepository, PlayerRepository);
            RemovePlayerFromRaidTeamHandler = new RemovePlayerFromRaidTeamHandler(GuildRepository);

            AddPlayerToGuildHandler = new AddPlayerToGuildHandler(GuildRepository, PlayerRepository);
            AddAdditionalRoleHandler = new AddAdditionalRoleToGuildMemberHandler(GuildRepository);
            RemoveAdditionalRoleHandler = new RemoveAdditionalRoleFromGuildMemberHandler(GuildRepository);
        }

        public InMemoryGuildRepository GuildRepository { get; } = new();

        public InMemoryPlayerRepository PlayerRepository { get; } = new();

        public CreateGuildHandler CreateGuildHandler { get; }
        public GetGuildHandler GetGuildHandler { get; }
        public ListGuildsHandler ListGuildsHandler { get; }
        public UpdateGuildHandler UpdateGuildHandler { get; }
        public DeleteGuildHandler DeleteGuildHandler { get; }

        public CreatePlayerHandler CreatePlayerHandler { get; }
        public GetPlayerHandler GetPlayerHandler { get; }
        public ListPlayersHandler ListPlayersHandler { get; }
        public UpdatePlayerHandler UpdatePlayerHandler { get; }
        public DeletePlayerHandler DeletePlayerHandler { get; }

        public CreateCharacterHandler CreateCharacterHandler { get; }
        public GetCharacterHandler GetCharacterHandler { get; }
        public ListCharactersHandler ListCharactersHandler { get; }
        public ListCharactersForPlayerHandler ListCharactersForPlayerHandler { get; }
        public UpdateCharacterHandler UpdateCharacterHandler { get; }
        public DeleteCharacterHandler DeleteCharacterHandler { get; }
        public SetMainCharacterHandler SetMainCharacterHandler { get; }

        public CreateRaidTeamHandler CreateRaidTeamHandler { get; }
        public GetRaidTeamHandler GetRaidTeamHandler { get; }
        public ListRaidTeamsForGuildHandler ListRaidTeamsForGuildHandler { get; }
        public UpdateRaidTeamHandler UpdateRaidTeamHandler { get; }
        public DeleteRaidTeamHandler DeleteRaidTeamHandler { get; }
        public AddPlayerToRaidTeamHandler AddPlayerToRaidTeamHandler { get; }
        public RemovePlayerFromRaidTeamHandler RemovePlayerFromRaidTeamHandler { get; }

        public AddPlayerToGuildHandler AddPlayerToGuildHandler { get; }
        public AddAdditionalRoleToGuildMemberHandler AddAdditionalRoleHandler { get; }
        public RemoveAdditionalRoleFromGuildMemberHandler RemoveAdditionalRoleHandler { get; }
    }

    private sealed class InMemoryGuildRepository : IGuildRepository
    {
        private readonly Dictionary<Guid, Guild> _guilds = new();

        public Guild? GetById(Guid id)
            => _guilds.TryGetValue(id, out var guild) ? guild : null;

        public IReadOnlyCollection<Guild> List()
            => _guilds.Values.ToList();

        public void Add(Guild guild)
            => _guilds.Add(guild.Id, guild);

        public void Remove(Guid id)
            => _guilds.Remove(id);

        public void SaveChanges()
        {
        }
    }

    private sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<Guid, Player> _players = new();

        public Player? GetById(Guid id)
            => _players.TryGetValue(id, out var player) ? player : null;

        public IReadOnlyCollection<Player> List()
            => _players.Values.ToList();

        public void Add(Player player)
            => _players.Add(player.Id, player);

        public void Remove(Guid id)
            => _players.Remove(id);

        public void SaveChanges()
        {
        }
    }
}
