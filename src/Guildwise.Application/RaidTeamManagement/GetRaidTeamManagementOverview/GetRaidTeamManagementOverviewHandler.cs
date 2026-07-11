using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Contracts.RaidTeamManagement;
using Guildwise.Domain;

namespace Guildwise.Application.RaidTeamManagement.GetRaidTeamManagementOverview;

public sealed class GetRaidTeamManagementOverviewHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetRaidTeamManagementOverviewHandler(IGuildRepository guildRepository, IPlayerRepository playerRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<RaidTeamManagementOverviewDto> HandleAsync(
        GetRaidTeamManagementOverviewQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var guilds = (await _guildRepository.ListAsync(cancellationToken))
            .OrderBy(guild => guild.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(guild => guild.Id)
            .ToList();

        var players = (await _playerRepository.ListAsync(cancellationToken))
            .ToDictionary(player => player.Id);

        var guildDtos = guilds
            .Select(guild => ToGuildDto(guild, players))
            .ToList();

        return new RaidTeamManagementOverviewDto(guildDtos);
    }

    private static RaidTeamManagementGuildDto ToGuildDto(
        Guild guild,
        Dictionary<Guid, Player> players)
    {
        var guildMemberIds = guild.Members
            .Select(member => member.PlayerId)
            .ToHashSet();

        var raidMemberIds = guild.RaidTeams
            .SelectMany(team => team.Members)
            .Select(member => member.PlayerId)
            .Where(guildMemberIds.Contains)
            .ToHashSet();

        var teams = guild.RaidTeams
            .OrderBy(team => team.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(team => team.Id)
            .Select(team => ToTeamDto(guild, team, players))
            .ToList();

        var playersWithoutMainCharacterCount = guild.Members
            .Count(member => !players.TryGetValue(member.PlayerId, out var player) || !player.MainCharacterId.HasValue);

        return new RaidTeamManagementGuildDto(
            guild.Id,
            guild.Name,
            guild.Region,
            guild.Realm,
            guild.RaidTeams.Count,
            guild.Members.Count,
            raidMemberIds.Count,
            guild.Members.Count(member => !raidMemberIds.Contains(member.PlayerId)),
            playersWithoutMainCharacterCount,
            teams);
    }

    private static RaidTeamManagementTeamDto ToTeamDto(
        Guild guild,
        RaidTeam raidTeam,
        Dictionary<Guid, Player> players)
    {
        var members = raidTeam.Members
            .Select(member => ToMemberDto(guild, member.PlayerId, players))
            .OrderBy(member => member.PlayerDisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(member => member.PlayerId)
            .ToList();

        var composition = new RaidTeamCompositionDto(
            members.Count(member => member.HasMainCharacter && member.Role == CharacterRole.Tank),
            members.Count(member => member.HasMainCharacter && member.Role == CharacterRole.Healer),
            members.Count(member => member.HasMainCharacter && member.Role == CharacterRole.Damage));

        return new RaidTeamManagementTeamDto(
            raidTeam.Id,
            raidTeam.GuildId,
            raidTeam.Name,
            members.Count,
            composition,
            members);
    }

    private static RaidTeamManagementMemberDto ToMemberDto(
        Guild guild,
        Guid playerId,
        Dictionary<Guid, Player> players)
    {
        players.TryGetValue(playerId, out var player);

        var guildMember = guild.Members.FirstOrDefault(member => member.PlayerId == playerId);
        var mainCharacter = player?.MainCharacterId.HasValue == true
            ? player.Characters.FirstOrDefault(character => character.Id == player.MainCharacterId.Value)
            : null;

        return new RaidTeamManagementMemberDto(
            playerId,
            player?.DisplayName ?? playerId.ToString("N"),
            mainCharacter?.Id,
            mainCharacter?.Name,
            mainCharacter?.Region,
            mainCharacter?.Realm,
            mainCharacter?.CharacterClass,
            mainCharacter?.Specialization,
            mainCharacter?.Role,
            mainCharacter is not null,
            guildMember?.Rank,
            guildMember?.AdditionalRoles.ToList() ?? []);
    }
}
