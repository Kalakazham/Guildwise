namespace Guildwise.Domain;

public sealed class RaidEvent
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid GuildId { get; private set; }

    public Guid RaidTeamId { get; private set; }

    public string Title { get; private set; }

    public DateTimeOffset StartTime { get; private set; }

    public DateTimeOffset? EndTime { get; private set; }

    public string InstanceName { get; private set; }

    public RaidDifficulty Difficulty { get; private set; }

    public string Notes { get; private set; }

    public RaidEventStatus Status { get; private set; }

    private RaidEvent(
        Guid guildId,
        Guid raidTeamId,
        string title,
        DateTimeOffset startTime,
        DateTimeOffset? endTime,
        string instanceName,
        RaidDifficulty difficulty,
        string? notes,
        RaidEventStatus status = RaidEventStatus.Scheduled)
    {
        ApplyDetails(guildId, raidTeamId, title, startTime, endTime, instanceName, difficulty, notes);
        DomainGuard.RequiredEnum(status, nameof(status));
        Status = status;
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

    public void UpdateDetails(
        Guid guildId,
        Guid raidTeamId,
        string title,
        DateTimeOffset startTime,
        DateTimeOffset? endTime,
        string instanceName,
        RaidDifficulty difficulty,
        string? notes)
    {
        if (Status == RaidEventStatus.Cancelled)
        {
            throw new InvalidOperationException("Cancelled raid events cannot be updated.");
        }

        ApplyDetails(guildId, raidTeamId, title, startTime, endTime, instanceName, difficulty, notes);
    }

    public void Cancel()
    {
        if (Status == RaidEventStatus.Cancelled)
        {
            return;
        }

        Status = RaidEventStatus.Cancelled;
    }

    private void ApplyDetails(
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
        StartTime = startTime.ToUniversalTime();
        EndTime = endTime?.ToUniversalTime();
        InstanceName = DomainGuard.Required(instanceName, nameof(instanceName));
        Difficulty = difficulty;
        Notes = notes?.Trim() ?? string.Empty;
    }
}
