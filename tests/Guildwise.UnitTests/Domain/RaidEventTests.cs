using System.Reflection;
using Guildwise.Domain;
using static Guildwise.UnitTests.DomainModelTestSupport;
namespace Guildwise.UnitTests;

public sealed class RaidEventTests
{
    [Fact]
    public void RaidEvent_Create_Creates_Valid_Event()
    {
        var guildId = Guid.NewGuid();
        var raidTeamId = Guid.NewGuid();
        var startTime = DateTimeOffset.UtcNow.AddDays(1);
        var endTime = startTime.AddHours(3);

        var raidEvent = RaidEvent.Create(
            guildId,
            raidTeamId,
            " Liberation of Undermine ",
            startTime,
            endTime,
            " Liberation of Undermine ",
            RaidDifficulty.Heroic,
            "  Bring flasks. ");

        Assert.Equal(guildId, raidEvent.GuildId);
        Assert.Equal(raidTeamId, raidEvent.RaidTeamId);
        Assert.Equal("Liberation of Undermine", raidEvent.Title);
        Assert.Equal(startTime, raidEvent.StartTime);
        Assert.Equal(endTime, raidEvent.EndTime);
        Assert.Equal("Liberation of Undermine", raidEvent.InstanceName);
        Assert.Equal(RaidDifficulty.Heroic, raidEvent.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status);
        Assert.Equal("Bring flasks.", raidEvent.Notes);
    }

    [Fact]
    public void RaidEvent_Create_Normalizes_TimeValues_To_Utc()
    {
        var startTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));
        var endTime = startTime.AddHours(3);

        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            endTime,
            "Nerubar Palace",
            RaidDifficulty.Heroic,
            null);

        Assert.Equal(TimeSpan.Zero, raidEvent.StartTime.Offset);
        Assert.Equal(startTime.ToUniversalTime(), raidEvent.StartTime);
        Assert.NotNull(raidEvent.EndTime);
        Assert.Equal(TimeSpan.Zero, raidEvent.EndTime.Value.Offset);
        Assert.Equal(endTime.ToUniversalTime(), raidEvent.EndTime.Value);
    }

    [Fact]
    public void RaidEvent_UpdateDetails_Updates_Event_Details()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);
        var newGuildId = Guid.NewGuid();
        var newRaidTeamId = Guid.NewGuid();
        var newStartTime = new DateTimeOffset(2026, 7, 13, 20, 30, 0, TimeSpan.FromHours(2));
        var newEndTime = newStartTime.AddHours(3);

        raidEvent.UpdateDetails(
            newGuildId,
            newRaidTeamId,
            " Manaforge Omega ",
            newStartTime,
            newEndTime,
            " Manaforge Omega ",
            RaidDifficulty.Mythic,
            "  Bring cauldrons. ");

        Assert.Equal(newGuildId, raidEvent.GuildId);
        Assert.Equal(newRaidTeamId, raidEvent.RaidTeamId);
        Assert.Equal("Manaforge Omega", raidEvent.Title);
        Assert.Equal(TimeSpan.Zero, raidEvent.StartTime.Offset);
        Assert.Equal(newStartTime.ToUniversalTime(), raidEvent.StartTime);
        Assert.NotNull(raidEvent.EndTime);
        Assert.Equal(TimeSpan.Zero, raidEvent.EndTime.Value.Offset);
        Assert.Equal(newEndTime.ToUniversalTime(), raidEvent.EndTime.Value);
        Assert.Equal("Manaforge Omega", raidEvent.InstanceName);
        Assert.Equal(RaidDifficulty.Mythic, raidEvent.Difficulty);
        Assert.Equal(RaidEventStatus.Scheduled, raidEvent.Status);
        Assert.Equal("Bring cauldrons.", raidEvent.Notes);
    }

    [Fact]
    public void RaidEvent_UpdateDetails_Rejects_Cancelled_Event()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);
        raidEvent.Cancel();

        Assert.Throws<InvalidOperationException>(() => raidEvent.UpdateDetails(
            raidEvent.GuildId,
            raidEvent.RaidTeamId,
            "Updated",
            DateTimeOffset.UtcNow.AddDays(2),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Cancel_Sets_Status_To_Cancelled()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);

        raidEvent.Cancel();

        Assert.Equal(RaidEventStatus.Cancelled, raidEvent.Status);
    }

    [Fact]
    public void RaidEvent_Cancel_When_Already_Cancelled_Is_Idempotent()
    {
        var raidEvent = RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null);

        raidEvent.Cancel();
        raidEvent.Cancel();

        Assert.Equal(RaidEventStatus.Cancelled, raidEvent.Status);
    }

    [Fact]
    public void RaidEvent_SetSignup_Adds_New_Signup()
    {
        var raidEvent = CreateRaidEvent();
        var playerId = Guid.NewGuid();

        var signup = raidEvent.SetSignup(playerId, RaidEventSignupStatus.Signed);

        Assert.Equal(raidEvent.Id, signup.RaidEventId);
        Assert.Equal(playerId, signup.PlayerId);
        Assert.Equal(RaidEventSignupStatus.Signed, signup.Status);
        Assert.Same(signup, Assert.Single(raidEvent.Signups));
    }

    [Fact]
    public void RaidEvent_SetSignup_Updates_Existing_Signup_For_Player()
    {
        var raidEvent = CreateRaidEvent();
        var playerId = Guid.NewGuid();
        var initial = raidEvent.SetSignup(playerId, RaidEventSignupStatus.Signed);

        var updated = raidEvent.SetSignup(playerId, RaidEventSignupStatus.Tentative);

        Assert.Same(initial, updated);
        Assert.Single(raidEvent.Signups);
        Assert.Equal(RaidEventSignupStatus.Tentative, updated.Status);
    }

    [Fact]
    public void RaidEvent_SetSignup_Rejects_Empty_PlayerId()
    {
        var raidEvent = CreateRaidEvent();

        Assert.Throws<ArgumentException>(() => raidEvent.SetSignup(Guid.Empty, RaidEventSignupStatus.Signed));
    }

    [Fact]
    public void RaidEvent_SetSignup_Rejects_Unknown_And_Undefined_Status()
    {
        var raidEvent = CreateRaidEvent();
        var playerId = Guid.NewGuid();

        Assert.Throws<ArgumentOutOfRangeException>(() => raidEvent.SetSignup(playerId, RaidEventSignupStatus.Unknown));
        Assert.Throws<ArgumentOutOfRangeException>(() => raidEvent.SetSignup(playerId, (RaidEventSignupStatus)999));
    }

    [Fact]
    public void RaidEvent_SetSignup_Rejects_Cancelled_Event()
    {
        var raidEvent = CreateRaidEvent();
        raidEvent.Cancel();

        Assert.Throws<InvalidOperationException>(() => raidEvent.SetSignup(Guid.NewGuid(), RaidEventSignupStatus.Signed));
    }

    [Fact]
    public void RaidEvent_Signups_Collection_Is_Not_Publicly_Mutable()
    {
        var raidEvent = CreateRaidEvent();
        raidEvent.SetSignup(Guid.NewGuid(), RaidEventSignupStatus.Signed);

        Assert.IsAssignableFrom<IReadOnlyCollection<RaidEventSignup>>(raidEvent.Signups);
        Assert.False(raidEvent.Signups is ICollection<RaidEventSignup> { IsReadOnly: false });
    }

    [Fact]
    public void RaidEvent_Missing_Response_Is_Not_Stored_As_Signup()
    {
        var raidEvent = CreateRaidEvent();

        Assert.Empty(raidEvent.Signups);
        Assert.DoesNotContain(RaidEventSignupStatus.Unknown, raidEvent.Signups.Select(signup => signup.Status));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Empty_Guild_Or_RaidTeam()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.Empty,
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));

        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.Empty,
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Blank_Title()
    {
        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            " ",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Blank_InstanceName()
    {
        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            DateTimeOffset.UtcNow.AddDays(1),
            null,
            " ",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Default_StartTime()
    {
        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            default,
            null,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_EndTime_Not_After_StartTime()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            startTime,
            "Nerubar Palace",
            RaidDifficulty.Normal,
            null));
    }

    [Fact]
    public void RaidEvent_Create_Rejects_Unknown_And_Undefined_Difficulty()
    {
        var startTime = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            RaidDifficulty.Unknown,
            null));

        Assert.Throws<ArgumentOutOfRangeException>(() => RaidEvent.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Raid Night",
            startTime,
            null,
            "Nerubar Palace",
            (RaidDifficulty)999,
            null));
    }
}
