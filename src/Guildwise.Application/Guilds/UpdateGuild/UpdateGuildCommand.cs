namespace Guildwise.Application.Guilds.UpdateGuild;

public sealed record UpdateGuildCommand(Guid GuildId, string Name, string Region, string Realm);
