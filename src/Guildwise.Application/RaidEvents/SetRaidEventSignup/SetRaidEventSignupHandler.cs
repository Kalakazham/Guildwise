using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.RaidEvents;
using Guildwise.Domain;

namespace Guildwise.Application.RaidEvents.SetRaidEventSignup;

public sealed class SetRaidEventSignupHandler
{
    private readonly IGuildRepository _guildRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IRaidEventRepository _raidEventRepository;

    public SetRaidEventSignupHandler(
        IGuildRepository guildRepository,
        IPlayerRepository playerRepository,
        IRaidEventRepository raidEventRepository)
    {
        _guildRepository = guildRepository ?? throw new ArgumentNullException(nameof(guildRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _raidEventRepository = raidEventRepository ?? throw new ArgumentNullException(nameof(raidEventRepository));
    }

    public async Task<Result<RaidEventSignupDto>> HandleAsync(
        SetRaidEventSignupCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var validation = Validate(command);
        if (validation is not null)
        {
            return Result<RaidEventSignupDto>.Validation(validation);
        }

        var raidEvent = await _raidEventRepository.GetByIdAsync(command.RaidEventId, cancellationToken);
        if (raidEvent is null)
        {
            return Result<RaidEventSignupDto>.NotFound($"RaidEvent '{command.RaidEventId}' was not found.");
        }

        if (raidEvent.Status != RaidEventStatus.Scheduled)
        {
            return Result<RaidEventSignupDto>.BusinessRule("Signups can only be changed for scheduled raid events.");
        }

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result<RaidEventSignupDto>.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        var guild = await _guildRepository.GetByIdAsync(raidEvent.GuildId, cancellationToken);
        if (guild is null)
        {
            return Result<RaidEventSignupDto>.NotFound($"Guild '{raidEvent.GuildId}' was not found.");
        }

        var guildMember = guild.Members.FirstOrDefault(member => member.PlayerId == player.Id);
        if (guildMember is null)
        {
            return Result<RaidEventSignupDto>.BusinessRule("Player must be a guild member for the raid event guild.");
        }

        var signup = raidEvent.SetSignup(player.Id, command.Status);
        await _raidEventRepository.SaveChangesAsync(cancellationToken);

        return Result<RaidEventSignupDto>.Success(DtoMapper.ToDto(raidEvent, signup, player, guildMember));
    }

    private static string? Validate(SetRaidEventSignupCommand command)
    {
        if (command.RaidEventId == Guid.Empty)
        {
            return "Raid event is required.";
        }

        if (command.PlayerId == Guid.Empty)
        {
            return "Player is required.";
        }

        if (EqualityComparer<RaidEventSignupStatus>.Default.Equals(command.Status, default)
            || !Enum.IsDefined(command.Status))
        {
            return "Raid event signup status is required.";
        }

        return null;
    }
}
