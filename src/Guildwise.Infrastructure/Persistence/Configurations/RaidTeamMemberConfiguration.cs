using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class RaidTeamMemberConfiguration : IEntityTypeConfiguration<RaidTeamMember>
{
    public void Configure(EntityTypeBuilder<RaidTeamMember> builder)
    {
        builder.ToTable("raid_team_members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(member => member.RaidTeamId)
            .HasColumnName("raid_team_id");

        builder.Property(member => member.PlayerId)
            .HasColumnName("player_id");

        builder.HasIndex(member => new { member.RaidTeamId, member.PlayerId })
            .IsUnique();

        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(member => member.PlayerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
