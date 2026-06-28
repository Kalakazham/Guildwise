using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class GuildMemberConfiguration : IEntityTypeConfiguration<GuildMember>
{
    public void Configure(EntityTypeBuilder<GuildMember> builder)
    {
        builder.ToTable("guild_members");

        builder.HasKey(member => member.Id);

        builder.Property(member => member.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(member => member.GuildId)
            .HasColumnName("guild_id");

        builder.Property(member => member.PlayerId)
            .HasColumnName("player_id");

        builder.Property(member => member.Rank)
            .HasColumnName("guild_rank")
            .HasMaxLength(30)
            .HasConversion<string>()
            .IsRequired();

        builder.PrimitiveCollection(member => member.AdditionalRoles)
            .HasColumnName("additional_roles")
            .ElementType()
            .HasConversion<string>();

        builder.HasIndex(member => new { member.GuildId, member.PlayerId })
            .IsUnique();

        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(member => member.PlayerId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
