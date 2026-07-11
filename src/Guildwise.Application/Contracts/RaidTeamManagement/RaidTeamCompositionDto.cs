namespace Guildwise.Application.Contracts.RaidTeamManagement;

public sealed record RaidTeamCompositionDto(
    int TankCount,
    int HealerCount,
    int DamageCount);
