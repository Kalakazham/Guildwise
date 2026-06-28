namespace Guildwise.Application.Players.UpdatePlayer;

public sealed record UpdatePlayerCommand(Guid PlayerId, string DisplayName);
