using Guildwise.Application.Abstractions.Persistence;
using Guildwise.Application.Common;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Characters;
using Guildwise.Domain;

namespace Guildwise.Application.Characters.CreateCharacter;

public sealed class CreateCharacterHandler
{
    private readonly IPlayerRepository _playerRepository;

    public CreateCharacterHandler(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
    }

    public async Task<Result<CharacterDto>> HandleAsync(
        CreateCharacterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var player = await _playerRepository.GetByIdAsync(command.PlayerId, cancellationToken);
        if (player is null)
        {
            return Result<CharacterDto>.NotFound($"Player '{command.PlayerId}' was not found.");
        }

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<CharacterDto>.Validation("Character name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Region))
        {
            return Result<CharacterDto>.Validation("Character region is required.");
        }

        if (string.IsNullOrWhiteSpace(command.Realm))
        {
            return Result<CharacterDto>.Validation("Character realm is required.");
        }

        if (!IsDefined(command.CharacterClass))
        {
            return Result<CharacterDto>.Validation("Character class is required.");
        }

        if (!IsDefined(command.Specialization))
        {
            return Result<CharacterDto>.Validation("Character specialization is required.");
        }

        if (!IsDefined(command.Role))
        {
            return Result<CharacterDto>.Validation("Character role is required.");
        }

        if (player.Characters.Any(character => HasSameCharacterIdentity(command, character)))
        {
            return Result<CharacterDto>.Conflict("Duplicate character for this player.");
        }

        Character character;
        try
        {
            character = player.AddCharacter(
                command.Name,
                command.Region,
                command.Realm,
                command.CharacterClass,
                command.Specialization,
                command.Role);
        }
        catch (InvalidOperationException exception)
            when (exception.Message == "Character specialization must match character class.")
        {
            return Result<CharacterDto>.Validation(exception.Message);
        }

        await _playerRepository.SaveChangesAsync(cancellationToken);

        return Result<CharacterDto>.Success(DtoMapper.ToDto(character));
    }

    private static bool IsDefined<TEnum>(TEnum value)
        where TEnum : struct, Enum
        => !EqualityComparer<TEnum>.Default.Equals(value, default) && Enum.IsDefined(value);

    private static bool HasSameCharacterIdentity(CreateCharacterCommand command, Character character)
        => string.Equals(command.Name.Trim(), character.Name.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(command.Region.Trim(), character.Region.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(command.Realm.Trim(), character.Realm.Trim(), StringComparison.OrdinalIgnoreCase);
}
