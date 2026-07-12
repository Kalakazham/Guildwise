using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class RaidEventSignupConfiguration : IEntityTypeConfiguration<RaidEventSignup>
{
    public void Configure(EntityTypeBuilder<RaidEventSignup> builder)
    {
        builder.ToTable("raid_event_signups");

        builder.HasKey(signup => signup.Id);

        builder.Property(signup => signup.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(signup => signup.RaidEventId)
            .HasColumnName("raid_event_id");

        builder.Property(signup => signup.PlayerId)
            .HasColumnName("player_id");

        builder.Property(signup => signup.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.HasIndex(signup => new { signup.RaidEventId, signup.PlayerId })
            .IsUnique();

        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(signup => signup.PlayerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
