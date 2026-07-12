using Guildwise.Domain;

namespace Guildwise.Web.Components.Pages.RaidEventComponents;

public sealed record RaidTeamOption(
    Guid GuildId,
    Guid RaidTeamId,
    string RaidTeamName,
    string GuildName,
    string Region,
    string Realm);

public sealed record RaidEventFormSubmission(
    Guid GuildId,
    Guid RaidTeamId,
    string Title,
    DateTimeOffset StartTime,
    DateTimeOffset? EndTime,
    string InstanceName,
    RaidDifficulty Difficulty,
    string Notes);

public sealed record RaidEventActionResult(
    bool IsSuccess,
    string Message,
    string MessageStatus)
{
    public static RaidEventActionResult Success(string message) => new(true, message, "success");

    public static RaidEventActionResult Failure(string message) => new(false, message, "error");
}

public sealed record SignupStatusChangeRequest(
    Guid RaidEventId,
    Guid PlayerId,
    RaidEventSignupStatus Status,
    RaidEventSignupStatus? CurrentStatus);

public sealed record SignupManagementRow(
    Guid PlayerId,
    string PlayerDisplayName,
    string MainCharacterName,
    bool HasMainCharacter,
    CharacterClass? CharacterClass,
    CharacterRole? Role,
    GuildRank? GuildRank,
    IReadOnlyList<AdditionalGuildRole> AdditionalRoles,
    RaidEventSignupStatus? CurrentStatus,
    bool HasPlayerContext);
