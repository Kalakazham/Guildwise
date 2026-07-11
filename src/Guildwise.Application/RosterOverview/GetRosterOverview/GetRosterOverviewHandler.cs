using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Contracts.RosterOverview;
using Guildwise.Domain;

namespace Guildwise.Application.RosterOverview.GetRosterOverview;

public sealed class GetRosterOverviewHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;

    public GetRosterOverviewHandler(IGuildRepository guildRepository, IPlayerRepository playerRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<RosterOverviewDto> HandleAsync(
        GetRosterOverviewQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var guilds = (await _guildRepository.ListAsync(cancellationToken)).ToList();
        var players = (await _playerRepository.ListAsync(cancellationToken)).ToList();

        var guildMemberLookup = guilds
            .SelectMany(guild => guild.Members)
            .GroupBy(member => member.PlayerId)
            .ToDictionary(
                group => group.Key,
                group => group.First());

        var raidTeamLookup = guilds
            .SelectMany(guild => guild.RaidTeams)
            .SelectMany(raidTeam => raidTeam.Members.Select(member => new { member.PlayerId, RaidTeamName = raidTeam.Name }))
            .GroupBy(member => member.PlayerId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(member => member.RaidTeamName)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToList());

        var members = players
            .OrderBy(player => player.DisplayName, StringComparer.OrdinalIgnoreCase)
            .Select(player => ToRosterMember(player, guildMemberLookup, raidTeamLookup))
            .ToList();

        var summary = new RosterSummaryDto(
            guilds.Count,
            players.Count,
            players.Sum(player => player.Characters.Count),
            guilds.Sum(guild => guild.RaidTeams.Count),
            guilds.Sum(guild => guild.Members.Count),
            guilds
                .SelectMany(guild => guild.RaidTeams)
                .SelectMany(raidTeam => raidTeam.Members)
                .Select(member => member.PlayerId)
                .Distinct()
                .Count(),
            players.Count(player => player.MainCharacterId.HasValue));

        var guildSummaries = guilds
            .OrderBy(guild => guild.Name, StringComparer.OrdinalIgnoreCase)
            .Select(guild => new RosterGuildSummaryDto(
                guild.Id,
                guild.Name,
                guild.Region,
                guild.Realm,
                guild.RaidTeams.Count,
                guild.Members.Count))
            .ToList();

        return new RosterOverviewDto(summary, guildSummaries, members);
    }

    private static RosterMemberDto ToRosterMember(
        Player player,
        Dictionary<Guid, GuildMember> guildMemberLookup,
        Dictionary<Guid, IReadOnlyList<string>> raidTeamLookup)
    {
        var mainCharacter = player.MainCharacterId.HasValue
            ? player.Characters.FirstOrDefault(character => character.Id == player.MainCharacterId.Value)
            : null;

        var isGuildMember = guildMemberLookup.TryGetValue(player.Id, out var guildMember);
        var raidTeamNames = raidTeamLookup.TryGetValue(player.Id, out var teams)
            ? teams
            : [];

        return new RosterMemberDto(
            player.Id,
            player.DisplayName,
            mainCharacter?.Id,
            mainCharacter?.Name,
            mainCharacter?.Region,
            mainCharacter?.Realm,
            mainCharacter?.CharacterClass,
            mainCharacter?.Specialization,
            mainCharacter?.Role,
            mainCharacter is not null,
            isGuildMember,
            guildMember?.Rank,
            guildMember?.AdditionalRoles.ToList() ?? [],
            raidTeamNames);
    }
}
