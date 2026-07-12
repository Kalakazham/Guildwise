namespace Guildwise.Domain;

public sealed class RaidEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid GuildId { get; }

    public Guid RaidTeamId { get; }

    public string Title { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset? EndTime { get; private set; }

    public string InstanceName { get; private set; }

    public RaidDifficulty Difficulty { get; private set; }

    public string Notes { get; private set; }

    private RaidEvent(
        Guid guildId,
        Guid raidTeamId,
        string title,
        DateTimeOffset startTime,
        DateTimeOffset? endTime,
        string instanceName,
        RaidDifficulty difficulty,
        string? notes)
    {
        if (guildId == Guid.Empty)
        {
            throw new ArgumentException("guildId is required.", nameof(guildId));
        }

        if (raidTeamId == Guid.Empty)
        {
            throw new ArgumentException("raidTeamId is required.", nameof(raidTeamId));
        }

        if (startTime == default)
        {
            throw new ArgumentException("startTime is required.", nameof(startTime));
        }

        if (endTime.HasValue && endTime.Value <= startTime)
        {
            throw new ArgumentException("endTime must be after startTime.", nameof(endTime));
        }

        DomainGuard.RequiredEnum(difficulty, nameof(difficulty));

        GuildId = guildId;
        RaidTeamId = raidTeamId;
        Title = DomainGuard.Required(title, nameof(title));
        StartTime = startTime;
        EndTime = endTime;
        InstanceName = DomainGuard.Required(instanceName, nameof(instanceName));
        Difficulty = difficulty;
        Notes = notes?.Trim() ?? string.Empty;
    }

    public static RaidEvent Create(
        Guid guildId,
        Guid raidTeamId,
        string title,
        DateTimeOffset startTime,
        DateTimeOffset? endTime,
        string instanceName,
        RaidDifficulty difficulty,
        string? notes)
        => new(
            guildId,
            raidTeamId,
            title,
            startTime,
            endTime,
            instanceName,
            difficulty,
            notes);
}
