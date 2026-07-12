using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.RaidEvents;
using Guildwise.Domain;

namespace Guildwise.Application.RaidEvents.CreateRaidEvent;

public sealed class CreateRaidEventHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IRaidEventRepository _raidEventRepository;

    public CreateRaidEventHandler(IGuildRepository guildRepository, IRaidEventRepository raidEventRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _raidEventRepository = raidEventRepository ?? throw new ArgumentNullException(nameof(raidEventRepository));
    }

    public async Task<Result<RaidEventDto>> HandleAsync(
        CreateRaidEventCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var guild = await _guildRepository.GetByIdAsync(command.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result<RaidEventDto>.NotFound($"Guild '{command.GuildId}' was not found.");
        }

        var raidTeam = guild.RaidTeams.FirstOrDefault(existing => existing.Id == command.RaidTeamId);
        if (raidTeam is null)
        {
            return Result<RaidEventDto>.NotFound($"RaidTeam '{command.RaidTeamId}' was not found.");
        }

        var validation = Validate(command);
        if (validation is not null)
        {
            return Result<RaidEventDto>.Validation(validation);
        }

        var raidEvent = RaidEvent.Create(
            guild.Id,
            raidTeam.Id,
            command.Title,
            command.StartTime,
            command.EndTime,
            command.InstanceName,
            command.Difficulty,
            command.Notes);

        await _raidEventRepository.AddAsync(raidEvent, cancellationToken);
        return Result<RaidEventDto>.Success(DtoMapper.ToDto(raidEvent));
    }

    private static string? Validate(CreateRaidEventCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return "Raid event title is required.";
        }

        if (string.IsNullOrWhiteSpace(command.InstanceName))
        {
            return "Raid event instance name is required.";
        }

        if (command.StartTime == default)
        {
            return "Raid event start time is required.";
        }

        if (command.EndTime.HasValue && command.EndTime.Value <= command.StartTime)
        {
            return "Raid event end time must be after the start time.";
        }

        if (EqualityComparer<RaidDifficulty>.Default.Equals(command.Difficulty, default)
            || !Enum.IsDefined(command.Difficulty))
        {
            return "Raid event difficulty is required.";
        }

        return null;
    }
}
