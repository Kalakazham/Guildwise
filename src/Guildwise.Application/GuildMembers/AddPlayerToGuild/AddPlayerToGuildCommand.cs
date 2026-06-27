using Guildwise.Domain;

namespace Guildwise.Application.GuildMembers.AddPlayerToGuild;

public sealed record AddPlayerToGuildCommand(Guid GuildId, Guid PlayerId, GuildRank Rank);
