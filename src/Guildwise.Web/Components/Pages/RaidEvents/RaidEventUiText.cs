using System.Globalization;
using Guildwise.Application.Common.Results;
using Guildwise.Application.Contracts.Guilds;
using Guildwise.Application.Contracts.RaidEvents;
using Guildwise.Application.Contracts.RaidTeams;
using Guildwise.Domain;

namespace Guildwise.Web.Components.Pages.RaidEventComponents;

internal static class RaidEventUiText
{
    private static readonly string[] AcceptedDateTimeLocalFormats =
    [
        "yyyy-MM-ddTHH:mm",
        "yyyy-MM-ddTHH:mm:ss",
        "dd.MM.yyyy HH:mm",
        "dd.MM.yyyy HH:mm:ss"
    ];

    public static string GetGuildName(GuildDto? guild)
        => guild?.Name ?? "Unknown guild context";

    public static string GetGuildRealm(GuildDto? guild)
        => guild is null ? "Unknown realm" : $"{guild.Region} - {guild.Realm}";

    public static string GetGuildContext(GuildDto? guild)
        => guild is null ? "Unknown guild context" : $"{guild.Name} - {guild.Region} - {guild.Realm}";

    public static string GetRaidTeamName(RaidTeamDto? raidTeam)
        => raidTeam?.Name ?? "Unknown raid team";

    public static string FormatRaidTeamOption(RaidTeamOption option)
        => $"{option.RaidTeamName} - {option.GuildName} - {option.Region} - {option.Realm}";

    public static string FormatDateTime(DateTimeOffset value)
        => value.ToLocalTime().ToString("ddd, dd MMM yyyy HH:mm");

    public static string FormatOptionalDateTime(DateTimeOffset? value)
        => value.HasValue ? FormatDateTime(value.Value) : "Not set";

    public static string FormatSummaryDate(DateTimeOffset? value)
        => value.HasValue ? value.Value.ToLocalTime().ToString("dd MMM") : "-";

    public static string FormatDateTimeLocalValue(DateTimeOffset value)
        => value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture);

    public static string GetNextEventDetail(RaidEventDto? raidEvent)
        => raidEvent is null ? "No planned event" : raidEvent.Title;

    public static string FormatDuration(RaidEventDto raidEvent)
    {
        if (!raidEvent.EndTime.HasValue)
        {
            return "Not set";
        }

        var duration = raidEvent.EndTime.Value - raidEvent.StartTime;
        if (duration.TotalMinutes < 1)
        {
            return "Not set";
        }

        return duration.TotalHours >= 1
            ? $"{(int)duration.TotalHours}h {duration.Minutes}m"
            : $"{duration.Minutes}m";
    }

    public static bool TryParseLocalDateTime(string value, out DateTimeOffset dateTimeOffset)
    {
        dateTimeOffset = default;
        if (!TryParseDateTimeLocalInput(value, out var localDateTime))
        {
            return false;
        }

        localDateTime = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        dateTimeOffset = new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));
        return true;
    }

    public static string NormalizeDateTimeLocalInput(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return TryParseDateTimeLocalInput(value, out var localDateTime)
            ? localDateTime.ToString("yyyy-MM-ddTHH:mm", CultureInfo.InvariantCulture)
            : value;
    }

    public static string FormatFailure(Failure failure)
        => $"{FormatFailureType(failure.Type)}: {failure.Message}";

    public static string FormatFailureType(FailureType type)
        => type switch
        {
            FailureType.NotFound => "Not found",
            FailureType.Validation => "Validation",
            FailureType.Conflict => "Conflict",
            FailureType.BusinessRule => "Cannot complete action",
            _ => "Failure"
        };

    public static string FormatEventStatus(RaidEventStatus status)
        => status switch
        {
            RaidEventStatus.Scheduled => "Scheduled",
            RaidEventStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };

    public static string GetEventStatusClass(RaidEventStatus status, bool large = false)
    {
        var statusToken = status switch
        {
            RaidEventStatus.Scheduled => "scheduled",
            RaidEventStatus.Cancelled => "cancelled",
            _ => "unknown"
        };

        return large
            ? $"event-status-pill event-status-pill--{statusToken} event-status-pill--large"
            : $"event-status-pill event-status-pill--{statusToken}";
    }

    public static string GetEventListItemClass(RaidEventDto raidEvent, bool isSelected)
    {
        var classes = "event-list-item";
        if (isSelected)
        {
            classes += " event-list-item--selected";
        }

        if (raidEvent.Status == RaidEventStatus.Cancelled)
        {
            classes += " event-list-item--cancelled";
        }

        return classes;
    }

    public static string FormatSignupStatus(RaidEventSignupStatus status)
        => status switch
        {
            RaidEventSignupStatus.Signed => "Signed",
            RaidEventSignupStatus.Tentative => "Tentative",
            RaidEventSignupStatus.Declined => "Declined",
            _ => "Unknown"
        };

    public static string FormatSignupStatus(RaidEventSignupStatus? status)
        => status.HasValue ? FormatSignupStatus(status.Value) : "Missing response";

    public static string GetSignupStatusClass(RaidEventSignupStatus status)
    {
        var statusToken = status switch
        {
            RaidEventSignupStatus.Signed => "signed",
            RaidEventSignupStatus.Tentative => "tentative",
            RaidEventSignupStatus.Declined => "declined",
            _ => "unknown"
        };

        return $"event-signup-status event-signup-status--{statusToken}";
    }

    public static string GetSignupStatusClass(RaidEventSignupStatus? status)
        => status.HasValue
            ? GetSignupStatusClass(status.Value)
            : "event-signup-status event-signup-status--missing";

    private static bool TryParseDateTimeLocalInput(string value, out DateTime localDateTime)
    {
        localDateTime = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (DateTime.TryParseExact(
                trimmed,
                AcceptedDateTimeLocalFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out localDateTime))
        {
            return true;
        }

        return DateTime.TryParse(
            trimmed,
            CultureInfo.CurrentCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out localDateTime);
    }
}
