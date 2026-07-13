using Guildwise.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Guildwise.Infrastructure.Persistence.Configurations;

public sealed class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("characters");

        builder.HasKey(character => character.Id);

        builder.Property(character => character.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(character => character.PlayerId)
            .HasColumnName("player_id");

        builder.Property(character => character.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(character => character.Region)
            .HasColumnName("region")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(character => character.Realm)
            .HasColumnName("realm")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(character => character.CharacterClass)
            .HasColumnName("character_class")
            .HasMaxLength(50)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(character => character.Specialization)
            .HasColumnName("character_specialization")
            .HasMaxLength(80)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(character => character.Role)
            .HasColumnName("character_role")
            .HasMaxLength(30)
            .HasConversion<string>()
            .IsRequired();

        builder.HasIndex(character => new
        {
            character.PlayerId,
            character.Region,
            character.Realm,
            character.Name
        })
            .IsUnique();
    }
}
