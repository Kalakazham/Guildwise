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
    public async Task CreateGuild_Stores_Guild_And_Returns_Dto()
    {
        var context = new TestContext();

        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));

        Assert.Equal("Guildwise", guild.Name);
        Assert.Single(await context.GuildRepository.ListAsync());
        Assert.Equal(guild.Id, (await context.GetGuildHandler.HandleAsync(new GetGuildQuery(guild.Id)))?.Id);
    }

    [Fact]
    public async Task UpdateUnknownPlayer_Throws_NotFoundException()
    {
        var context = new TestContext();

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            context.UpdatePlayerHandler.HandleAsync(new UpdatePlayerCommand(Guid.NewGuid(), "Myrmi")));

        Assert.Contains("Player", exception.Message);
    }

    [Fact]
    public async Task CreateCharacterForUnknownPlayer_Throws_NotFoundException()
    {
        var context = new TestContext();

        await Assert.ThrowsAsync<NotFoundException>(() => context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            Guid.NewGuid(),
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage)));
    }

    [Fact]
    public async Task CreateRaidTeamForUnknownGuild_Throws_NotFoundException()
    {
        var context = new TestContext();

        await Assert.ThrowsAsync<NotFoundException>(() => context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(
            Guid.NewGuid(),
            "Team One")));
    }

    [Fact]
    public async Task AddPlayerToRaidTeam_When_Player_Is_Not_GuildMember_Fails()
    {
        var context = new TestContext();
        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));
        var character = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));
        await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id));
        var raidTeam = await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id)));

        Assert.Contains("guild member", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddPlayerToUnknownRaidTeam_Throws_NotFoundException()
    {
        var context = new TestContext();
        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));
        var character = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));
        await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id));
        await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));

        await Assert.ThrowsAsync<NotFoundException>(() => context.AddPlayerToRaidTeamHandler.HandleAsync(
            new AddPlayerToRaidTeamCommand(guild.Id, Guid.NewGuid(), player.Id)));
    }

    [Fact]
    public async Task CreateAndUpdatePlayer_Works_Through_Application_Handler()
    {
        var context = new TestContext();

        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));
        var updated = await context.UpdatePlayerHandler.HandleAsync(new UpdatePlayerCommand(player.Id, "Myrmi Two"));

        Assert.Equal("Myrmi Two", updated.DisplayName);
        Assert.Equal("Myrmi Two", (await context.GetPlayerHandler.HandleAsync(new GetPlayerQuery(player.Id)))?.DisplayName);
    }

    [Fact]
    public async Task CreateCharacter_UpdateCharacter_And_SetMainCharacter_Work()
    {
        var context = new TestContext();
        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));

        var character = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFrost,
            CharacterRole.Damage));

        var updated = await context.UpdateCharacterHandler.HandleAsync(new UpdateCharacterCommand(
            player.Id,
            character.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Mage,
            CharacterSpecialization.MageFire,
            CharacterRole.Damage));

        var mainCharacter = await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, updated.Id));

        Assert.Equal(CharacterSpecialization.MageFire, updated.Specialization);
        Assert.Equal(updated.Id, mainCharacter.MainCharacterId);
        Assert.Single(await context.ListCharactersForPlayerHandler.HandleAsync(new ListCharactersForPlayerQuery(player.Id)));
    }

    [Fact]
    public async Task CreateRaidTeam_AddPlayerToGuild_And_AddPlayerToRaidTeam_Work()
    {
        var context = new TestContext();
        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));
        var character = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));

        await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id));
        await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));

        var raidTeam = await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One"));
        var roster = await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));

        Assert.Single(roster.Members);
        Assert.Equal(player.Id, roster.Members.Single().PlayerId);
        Assert.Single(await context.ListRaidTeamsForGuildHandler.HandleAsync(new ListRaidTeamsForGuildQuery(guild.Id)));
    }

    [Fact]
    public async Task RenameRaidTeam_And_DeleteRaidTeam_Work_Through_Guild()
    {
        var context = new TestContext();
        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var raidTeam = await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One"));

        var renamed = await context.UpdateRaidTeamHandler.HandleAsync(new UpdateRaidTeamCommand(guild.Id, raidTeam.Id, "Team Two"));
        await context.DeleteRaidTeamHandler.HandleAsync(new DeleteRaidTeamCommand(guild.Id, raidTeam.Id));

        Assert.Equal("Team Two", renamed.Name);
        Assert.Empty(await context.ListRaidTeamsForGuildHandler.HandleAsync(new ListRaidTeamsForGuildQuery(guild.Id)));
    }

    [Fact]
    public async Task GuildMember_Roles_Are_Managed_Through_Application_Handler()
    {
        var context = new TestContext();
        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));

        await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Officer));
        var added = await context.AddAdditionalRoleHandler.HandleAsync(new AddAdditionalRoleToGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead));
        var removed = await context.RemoveAdditionalRoleHandler.HandleAsync(new RemoveAdditionalRoleFromGuildMemberCommand(
            guild.Id,
            player.Id,
            AdditionalGuildRole.RaidLead));

        Assert.Single(added.AdditionalRoles);
        Assert.Empty(removed.AdditionalRoles);
    }

    [Fact]
    public async Task DeletePlayer_Removes_Guild_Memberships_And_RaidTeam_Memberships()
    {
        var context = new TestContext();
        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));
        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));
        var character = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            CharacterSpecialization.PaladinRetribution,
            CharacterRole.Damage));
        await context.SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id));
        await context.AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guild.Id, player.Id, GuildRank.Member));
        var raidTeam = await context.CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, "Team One"));
        await context.AddPlayerToRaidTeamHandler.HandleAsync(new AddPlayerToRaidTeamCommand(guild.Id, raidTeam.Id, player.Id));

        await context.DeletePlayerHandler.HandleAsync(new DeletePlayerCommand(player.Id));

        Assert.Null(await context.GetPlayerHandler.HandleAsync(new GetPlayerQuery(player.Id)));
        Assert.Empty((await context.GetGuildHandler.HandleAsync(new GetGuildQuery(guild.Id)))!.Members);
        Assert.Empty((await context.GetRaidTeamHandler.HandleAsync(new GetRaidTeamQuery(guild.Id, raidTeam.Id)))!.Members);
    }

    [Fact]
    public async Task DeleteGuild_Removes_Guild()
    {
        var context = new TestContext();
        var guild = await context.CreateGuildHandler.HandleAsync(new CreateGuildCommand("Guildwise", "EU", "Draenor"));

        await context.DeleteGuildHandler.HandleAsync(new DeleteGuildCommand(guild.Id));

        Assert.Null(await context.GetGuildHandler.HandleAsync(new GetGuildQuery(guild.Id)));
        Assert.Empty(await context.ListGuildsHandler.HandleAsync(new ListGuildsQuery()));
    }

    [Fact]
    public async Task CreateCharacter_Returns_Created_Character()
    {
        var context = new TestContext();
        var player = await context.CreatePlayerHandler.HandleAsync(new CreatePlayerCommand("Myrmi"));

        var character = await context.CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            "Alysa",
            "EU",
            "Draenor",
            CharacterClass.DeathKnight,
            CharacterSpecialization.DeathKnightFrost,
            CharacterRole.Damage));

        Assert.Equal(CharacterClass.DeathKnight, character.CharacterClass);
        Assert.Single(await context.ListCharactersHandler.HandleAsync(new ListCharactersQuery()));
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

        public Task<Guild?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_guilds.TryGetValue(id, out var guild) ? guild : null);

        public Task<IReadOnlyCollection<Guild>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Guild>>(_guilds.Values.ToList());

        public Task AddAsync(Guild guild, CancellationToken cancellationToken = default)
        {
            _guilds.Add(guild.Id, guild);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _guilds.Remove(id);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryPlayerRepository : IPlayerRepository
    {
        private readonly Dictionary<Guid, Player> _players = new();

        public Task<Player?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_players.TryGetValue(id, out var player) ? player : null);

        public Task<IReadOnlyCollection<Player>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Player>>(_players.Values.ToList());

        public Task AddAsync(Player player, CancellationToken cancellationToken = default)
        {
            _players.Add(player.Id, player);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _players.Remove(id);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}

