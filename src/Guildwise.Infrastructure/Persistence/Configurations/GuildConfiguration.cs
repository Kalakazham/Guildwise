using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class GuildConfiguration : IEntityTypeConfiguration<Guild>
{
    public void Configure(EntityTypeBuilder<Guild> builder)
    {
        builder.ToTable("guilds");

        builder.HasKey(guild => guild.Id);

        builder.Property(guild => guild.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(guild => guild.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(guild => guild.Region)
            .HasColumnName("region")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(guild => guild.Realm)
            .HasColumnName("realm")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasMany(guild => guild.Members)
            .WithOne()
            .HasForeignKey(member => member.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(guild => guild.RaidTeams)
            .WithOne()
            .HasForeignKey(raidTeam => raidTeam.GuildId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(guild => guild.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(guild => guild.RaidTeams)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
