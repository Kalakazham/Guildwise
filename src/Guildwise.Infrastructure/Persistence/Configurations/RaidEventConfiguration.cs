using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class RaidEventConfiguration : IEntityTypeConfiguration<RaidEvent>
{
    public void Configure(EntityTypeBuilder<RaidEvent> builder)
    {
        builder.ToTable("raid_events");

        builder.HasKey(raidEvent => raidEvent.Id);

        builder.Property(raidEvent => raidEvent.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(raidEvent => raidEvent.GuildId)
            .HasColumnName("guild_id");

        builder.Property(raidEvent => raidEvent.RaidTeamId)
            .HasColumnName("raid_team_id");

        builder.Property(raidEvent => raidEvent.Title)
            .HasColumnName("title")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(raidEvent => raidEvent.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(raidEvent => raidEvent.EndTime)
            .HasColumnName("end_time");

        builder.Property(raidEvent => raidEvent.InstanceName)
            .HasColumnName("instance_name")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(raidEvent => raidEvent.Difficulty)
            .HasColumnName("difficulty")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(raidEvent => raidEvent.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(raidEvent => raidEvent.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000)
            .IsRequired();

        builder.HasOne<Guild>()
            .WithMany()
            .HasForeignKey(raidEvent => raidEvent.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<RaidTeam>()
            .WithMany()
            .HasForeignKey(raidEvent => raidEvent.RaidTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(raidEvent => raidEvent.GuildId);

        builder.HasIndex(raidEvent => raidEvent.RaidTeamId);

        builder.HasIndex(raidEvent => raidEvent.StartTime);
    }
}
