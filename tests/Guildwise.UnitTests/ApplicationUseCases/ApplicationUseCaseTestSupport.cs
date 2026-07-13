using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.DeleteCharacter;
using Guildwise.Application.Characters.GetCharacter;
using Guildwise.Application.Characters.ListCharacters;
using Guildwise.Application.Characters.ListCharactersForPlayer;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.Characters.UpdateCharacter;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Characters;
using Guildwise.Application.Contracts.GuildMembers;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Application.Contracts.Players;
using Guildwise.Application.Contracts.RaidEvents;
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
using Guildwise.Application.RaidEvents.CancelRaidEvent;
using Guildwise.Application.RaidEvents.CreateRaidEvent;
using Guildwise.Application.RaidEvents.GetRaidEvent;
using Guildwise.Application.RaidEvents.ListRaidEventSignups;
using Guildwise.Application.RaidEvents.ListRaidEvents;
using Guildwise.Application.RaidEvents.SetRaidEventSignup;
using Guildwise.Application.RaidEvents.UpdateRaidEvent;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Application.RaidTeams.DeleteRaidTeam;
using Guildwise.Application.RaidTeams.GetRaidTeam;
using Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;
using Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;
using Guildwise.Application.RaidTeams.UpdateRaidTeam;
using Guildwise.Application.RaidTeamManagement.GetRaidTeamManagementOverview;
using Guildwise.Application.RosterOverview.GetRosterOverview;
using Guildwise.Domain;
using static Guildwise.UnitTests.ApplicationUseCaseTestSupport;
namespace Guildwise.UnitTests;

internal static class ApplicationUseCaseTestSupport
{
    public static T AssertSuccess<T>(Result<T> result)
    {
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Failure);
        Assert.NotNull(result.Value);
        return result.Value;
    }

    public static void AssertSuccess(Result result)
    {
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Failure);
    }

    public static void AssertFailure<T>(Result<T> result, FailureType expectedType, string expectedMessagePart)
    {
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.NotNull(result.Failure);
        Assert.Equal(expectedType, result.Failure.Type);
        Assert.Contains(expectedMessagePart, result.Failure.Message, StringComparison.OrdinalIgnoreCase);
    }

    public static void AssertFailure(Result result, FailureType expectedType, string expectedMessagePart)
    {
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Failure);
        Assert.Equal(expectedType, result.Failure.Type);
        Assert.Contains(expectedMessagePart, result.Failure.Message, StringComparison.OrdinalIgnoreCase);
    }

}
internal sealed class TestContext
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
        DeletePlayerHandler = new DeletePlayerHandler(GuildRepository, PlayerRepository, TransactionRunner);

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
        RemovePlayerFromRaidTeamHandler = new RemovePlayerFromRaidTeamHandler(GuildRepository, PlayerRepository);

        CreateRaidEventHandler = new CreateRaidEventHandler(GuildRepository, RaidEventRepository);
        GetRaidEventHandler = new GetRaidEventHandler(RaidEventRepository);
        ListRaidEventsHandler = new ListRaidEventsHandler(RaidEventRepository);
        UpdateRaidEventHandler = new UpdateRaidEventHandler(GuildRepository, RaidEventRepository);
        CancelRaidEventHandler = new CancelRaidEventHandler(RaidEventRepository);
        SetRaidEventSignupHandler = new SetRaidEventSignupHandler(
            GuildRepository,
            PlayerRepository,
            RaidEventRepository);
        ListRaidEventSignupsHandler = new ListRaidEventSignupsHandler(
            GuildRepository,
            PlayerRepository,
            RaidEventRepository);

        AddPlayerToGuildHandler = new AddPlayerToGuildHandler(GuildRepository, PlayerRepository);
        AddAdditionalRoleHandler = new AddAdditionalRoleToGuildMemberHandler(GuildRepository);
        RemoveAdditionalRoleHandler = new RemoveAdditionalRoleFromGuildMemberHandler(GuildRepository);

        GetRosterOverviewHandler = new GetRosterOverviewHandler(GuildRepository, PlayerRepository);
        GetRaidTeamManagementOverviewHandler = new GetRaidTeamManagementOverviewHandler(GuildRepository, PlayerRepository);
    }

    public InMemoryGuildRepository GuildRepository { get; } = new();

    public InMemoryPlayerRepository PlayerRepository { get; } = new();

    public InMemoryRaidEventRepository RaidEventRepository { get; } = new();

    public RecordingTransactionRunner TransactionRunner { get; } = new();

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

    public CreateRaidEventHandler CreateRaidEventHandler { get; }
    public GetRaidEventHandler GetRaidEventHandler { get; }
    public ListRaidEventsHandler ListRaidEventsHandler { get; }
    public UpdateRaidEventHandler UpdateRaidEventHandler { get; }
    public CancelRaidEventHandler CancelRaidEventHandler { get; }
    public SetRaidEventSignupHandler SetRaidEventSignupHandler { get; }
    public ListRaidEventSignupsHandler ListRaidEventSignupsHandler { get; }

    public AddPlayerToGuildHandler AddPlayerToGuildHandler { get; }
    public AddAdditionalRoleToGuildMemberHandler AddAdditionalRoleHandler { get; }
    public RemoveAdditionalRoleFromGuildMemberHandler RemoveAdditionalRoleHandler { get; }
    public GetRosterOverviewHandler GetRosterOverviewHandler { get; }
    public GetRaidTeamManagementOverviewHandler GetRaidTeamManagementOverviewHandler { get; }

    public async Task<(Guid PlayerId, Guid CharacterId)> CreateReadyGuildMemberAsync(
        Guid guildId,
        string playerName,
        CharacterRole role)
    {
        var player = AssertSuccess(await CreatePlayerHandler.HandleAsync(new CreatePlayerCommand(playerName)));
        var character = AssertSuccess(await CreateCharacterHandler.HandleAsync(new CreateCharacterCommand(
            player.Id,
            $"{playerName}main",
            "EU",
            "Draenor",
            CharacterClass.Paladin,
            role == CharacterRole.Tank
                ? CharacterSpecialization.PaladinProtection
                : CharacterSpecialization.PaladinRetribution,
            role)));

        AssertSuccess(await SetMainCharacterHandler.HandleAsync(new SetMainCharacterCommand(player.Id, character.Id)));
        AssertSuccess(await AddPlayerToGuildHandler.HandleAsync(new AddPlayerToGuildCommand(guildId, player.Id, GuildRank.Member)));

        return (player.Id, character.Id);
    }

    public async Task<RaidEventDto> CreateReadyRaidEventAsync(string raidTeamName, string title)
    {
        var guild = AssertSuccess(await CreateGuildHandler.HandleAsync(new CreateGuildCommand(
            $"Guild{Guid.NewGuid():N}",
            "EU",
            "Draenor")));
        var raidTeam = AssertSuccess(await CreateRaidTeamHandler.HandleAsync(new CreateRaidTeamCommand(guild.Id, raidTeamName)));

        return AssertSuccess(await CreateRaidEventHandler.HandleAsync(new CreateRaidEventCommand(
            guild.Id,
            raidTeam.Id,
            title,
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null)));
    }
}

internal sealed class InMemoryGuildRepository : IGuildRepository
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

internal sealed class InMemoryPlayerRepository : IPlayerRepository
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

internal sealed class InMemoryRaidEventRepository : IRaidEventRepository
{
    private readonly Dictionary<Guid, RaidEvent> _raidEvents = new();

    public Task<RaidEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_raidEvents.TryGetValue(id, out var raidEvent) ? raidEvent : null);

    public Task<IReadOnlyCollection<RaidEvent>> ListAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents().ToList());

    public Task<IReadOnlyCollection<RaidEvent>> ListForGuildAsync(
        Guid guildId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents()
            .Where(raidEvent => raidEvent.GuildId == guildId)
            .ToList());

    public Task<IReadOnlyCollection<RaidEvent>> ListForRaidTeamAsync(
        Guid raidTeamId,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyCollection<RaidEvent>>(OrderedRaidEvents()
            .Where(raidEvent => raidEvent.RaidTeamId == raidTeamId)
            .ToList());

    public Task AddAsync(RaidEvent raidEvent, CancellationToken cancellationToken = default)
    {
        _raidEvents.Add(raidEvent.Id, raidEvent);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    private IEnumerable<RaidEvent> OrderedRaidEvents()
        => _raidEvents.Values
            .OrderBy(raidEvent => raidEvent.StartTime)
            .ThenBy(raidEvent => raidEvent.Title)
            .ThenBy(raidEvent => raidEvent.Id);
}

internal sealed class RecordingTransactionRunner : ITransactionRunner
{
    public int ExecuteCalls { get; private set; }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        ExecuteCalls++;
        await operation(cancellationToken);
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        ExecuteCalls++;
        return await operation(cancellationToken);
    }
}
