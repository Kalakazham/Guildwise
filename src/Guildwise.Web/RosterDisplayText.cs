using System.Text.RegularExpressions;
using Guildwise.Domain;

namespace Guildwise.Web;

internal static partial class RosterDisplayText
{
    internal static string CharacterClass(CharacterClass characterClass)
        => SplitWords(characterClass.ToString());

    internal static string CharacterClassColor(CharacterClass characterClass)
        => characterClass switch
        {
            Guildwise.Domain.CharacterClass.DeathKnight => "#C41E3A",
            Guildwise.Domain.CharacterClass.DemonHunter => "#A330C9",
            Guildwise.Domain.CharacterClass.Druid => "#FF7C0A",
            Guildwise.Domain.CharacterClass.Evoker => "#33937F",
            Guildwise.Domain.CharacterClass.Hunter => "#AAD372",
            Guildwise.Domain.CharacterClass.Mage => "#3FC7EB",
            Guildwise.Domain.CharacterClass.Monk => "#00FF98",
            Guildwise.Domain.CharacterClass.Paladin => "#F48CBA",
            Guildwise.Domain.CharacterClass.Priest => "#FFFFFF",
            Guildwise.Domain.CharacterClass.Rogue => "#FFF468",
            Guildwise.Domain.CharacterClass.Shaman => "#0070DD",
            Guildwise.Domain.CharacterClass.Warlock => "#8788EE",
            Guildwise.Domain.CharacterClass.Warrior => "#C69B6D",
            _ => "#94A3B8"
        };

    internal static string CharacterRole(CharacterRole role)
        => role switch
        {
            Guildwise.Domain.CharacterRole.Tank => "Tank",
            Guildwise.Domain.CharacterRole.Healer => "Healer",
            Guildwise.Domain.CharacterRole.Damage => "DPS",
            _ => "Unknown"
        };

    internal static string CharacterRoleToken(CharacterRole role)
        => role switch
        {
            Guildwise.Domain.CharacterRole.Tank => "tank",
            Guildwise.Domain.CharacterRole.Healer => "healer",
            Guildwise.Domain.CharacterRole.Damage => "damage",
            _ => "unknown"
        };

    internal static string CharacterSpecialization(CharacterSpecialization specialization)
        => SplitWords(specialization.ToString());

    internal static string GuildRank(GuildRank rank)
        => rank switch
        {
            Domain.GuildRank.GuildLead => "Guild Lead",
            Domain.GuildRank.Officer => "Officer",
            Domain.GuildRank.Member => "Member",
            _ => SplitWords(rank.ToString())
        };

    internal static string AdditionalGuildRole(AdditionalGuildRole role)
        => role switch
        {
            Domain.AdditionalGuildRole.RaidLead => "Raid Lead",
            Domain.AdditionalGuildRole.Recruiter => "Recruiter",
            _ => SplitWords(role.ToString())
        };

    private static string SplitWords(string value)
        => WordBoundaryRegex().Replace(value, " ");

    [GeneratedRegex("(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])")]
    private static partial Regex WordBoundaryRegex();
}
