using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class RaidTeamConfiguration : IEntityTypeConfiguration<RaidTeam>
{
    public void Configure(EntityTypeBuilder<RaidTeam> builder)
    {
        builder.ToTable("raid_teams");

        builder.HasKey(raidTeam => raidTeam.Id);

        builder.Property(raidTeam => raidTeam.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(raidTeam => raidTeam.GuildId)
            .HasColumnName("guild_id");

        builder.Property(raidTeam => raidTeam.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasMany(raidTeam => raidTeam.Members)
            .WithOne()
            .HasForeignKey(member => member.RaidTeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(raidTeam => raidTeam.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(raidTeam => new { raidTeam.GuildId, raidTeam.Name })
            .IsUnique();
    }
}
