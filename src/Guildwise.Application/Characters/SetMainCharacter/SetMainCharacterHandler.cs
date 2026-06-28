using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Players;

namespace Guildwise.Application.Characters.SetMainCharacter;

public sealed class SetMainCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public SetMainCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<Result<PlayerDto>> HandleAsync(
        SetMainCharacterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result<PlayerDto>.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        var character = player.Characters.FirstOrDefault(existing => existing.Id == command.CharacterId);
        if (character is null)
        {
            return Result<PlayerDto>.NotFound($"Character '{command.CharacterId}' was not found.");
        }

        player.SetMainCharacter(character);
        await _playerRepository.SaveChangesAsync(cancellationToken);
        return Result<PlayerDto>.Success(DtoMapper.ToDto(player));
    }
}
