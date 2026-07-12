using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.RaidEvents;

namespace Guildwise.Application.RaidEvents.CancelRaidEvent;

public sealed class CancelRaidEventHandler
{
    private readonly IRaidEventRepository _raidEventRepository;

    public CancelRaidEventHandler(IRaidEventRepository raidEventRepository)
    {
        _raidEventRepository = raidEventRepository ?? throw new ArgumentNullException(nameof(raidEventRepository));
    }

    public async Task<Result<RaidEventDto>> HandleAsync(
        CancelRaidEventCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var raidEvent = await _raidEventRepository.GetByIdAsync(command.RaidEventId, cancellationToken);
        if (raidEvent is null)
        {
            return Result<RaidEventDto>.NotFound($"RaidEvent '{command.RaidEventId}' was not found.");
        }

        raidEvent.Cancel();
        await _raidEventRepository.SaveChangesAsync(cancellationToken);
        return Result<RaidEventDto>.Success(DtoMapper.ToDto(raidEvent));
    }
}
