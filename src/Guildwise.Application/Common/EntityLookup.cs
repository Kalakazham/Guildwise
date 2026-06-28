using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Domain;

namespace Guildwise.Application.Common;

internal static class EntityLookup
{
    internal static async Task<Guild> GetGuildOrThrowAsync(
        this IGuildRepository repository,
        Guid guildId,
        CancellationToken cancellationToken = default)
        => await repository.GetByIdAsync(guildId, cancellationToken)
            ?? throw new NotFoundException(nameof(Guild), guildId);

    internal static async Task<Player> GetPlayerOrThrowAsync(
        this IPlayerRepository repository,
        Guid playerId,
        CancellationToken cancellationToken = default)
        => await repository.GetByIdAsync(playerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Player), playerId);

    internal static RaidTeam GetRaidTeamOrThrow(this Guild guild, Guid raidTeamId)
        => guild.RaidTeams.FirstOrDefault(raidTeam => raidTeam.Id == raidTeamId)
            ?? throw new NotFoundException(nameof(RaidTeam), raidTeamId);

    internal static GuildMember GetGuildMemberOrThrow(this Guild guild, Guid playerId)
        => guild.Members.FirstOrDefault(member => member.PlayerId == playerId)
            ?? throw new NotFoundException(nameof(GuildMember), playerId);

    internal static Character GetCharacterOrThrow(this Player player, Guid characterId)
        => player.Characters.FirstOrDefault(character => character.Id == characterId)
            ?? throw new NotFoundException(nameof(Character), characterId);
}
