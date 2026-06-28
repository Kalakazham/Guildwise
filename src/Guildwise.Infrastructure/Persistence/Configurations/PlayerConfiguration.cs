using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");

        builder.HasKey(player => player.Id);

        builder.Property(player => player.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(player => player.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(player => player.MainCharacterId)
            .HasColumnName("main_character_id");

        builder.HasMany(player => player.Characters)
            .WithOne()
            .HasForeignKey(character => character.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Character>()
            .WithMany()
            .HasForeignKey(player => player.MainCharacterId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(player => player.Characters)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
