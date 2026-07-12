using Guildwise.Application.Characters.CreateCharacter;
using Guildwise.Application.Characters.DeleteCharacter;
using Guildwise.Application.Characters.GetCharacter;
using Guildwise.Application.Characters.ListCharacters;
using Guildwise.Application.Characters.ListCharactersForPlayer;
using Guildwise.Application.Characters.SetMainCharacter;
using Guildwise.Application.Characters.UpdateCharacter;
using Guildwise.Application.GuildMembers.AddAdditionalRoleToGuildMember;
using Guildwise.Application.GuildMembers.AddPlayerToGuild;
using Guildwise.Application.GuildMembers.RemoveAdditionalRoleFromGuildMember;
using Guildwise.Application.Guilds.CreateGuild;
using Guildwise.Application.Guilds.DeleteGuild;
using Guildwise.Application.Guilds.GetGuild;
using Guildwise.Application.Guilds.ListGuilds;
using Guildwise.Application.Guilds.UpdateGuild;
using Guildwise.Application.Players.CreatePlayer;
using Guildwise.Application.Players.DeletePlayer;
using Guildwise.Application.Players.GetPlayer;
using Guildwise.Application.Players.ListPlayers;
using Guildwise.Application.Players.UpdatePlayer;
using Guildwise.Application.RaidEvents.CreateRaidEvent;
using Guildwise.Application.RaidEvents.GetRaidEvent;
using Guildwise.Application.RaidEvents.ListRaidEvents;
using Guildwise.Application.RaidTeams.AddPlayerToRaidTeam;
using Guildwise.Application.RaidTeams.CreateRaidTeam;
using Guildwise.Application.RaidTeams.DeleteRaidTeam;
using Guildwise.Application.RaidTeams.GetRaidTeam;
using Guildwise.Application.RaidTeams.ListRaidTeamsForGuild;
using Guildwise.Application.RaidTeams.RemovePlayerFromRaidTeam;
using Guildwise.Application.RaidTeams.UpdateRaidTeam;
using Guildwise.Application.RaidTeamManagement.GetRaidTeamManagementOverview;
using Guildwise.Application.RosterOverview.GetRosterOverview;
using Microsoft.Extensions.DependencyInjection;

namespace Guildwise.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGuildwiseApplicationUseCases(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<CreateGuildHandler>();
        services.AddScoped<GetGuildHandler>();
        services.AddScoped<ListGuildsHandler>();
        services.AddScoped<UpdateGuildHandler>();
        services.AddScoped<DeleteGuildHandler>();

        services.AddScoped<CreatePlayerHandler>();
        services.AddScoped<GetPlayerHandler>();
        services.AddScoped<ListPlayersHandler>();
        services.AddScoped<UpdatePlayerHandler>();
        services.AddScoped<DeletePlayerHandler>();

        services.AddScoped<CreateCharacterHandler>();
        services.AddScoped<GetCharacterHandler>();
        services.AddScoped<ListCharactersHandler>();
        services.AddScoped<ListCharactersForPlayerHandler>();
        services.AddScoped<UpdateCharacterHandler>();
        services.AddScoped<DeleteCharacterHandler>();
        services.AddScoped<SetMainCharacterHandler>();

        services.AddScoped<CreateRaidTeamHandler>();
        services.AddScoped<GetRaidTeamHandler>();
        services.AddScoped<ListRaidTeamsForGuildHandler>();
        services.AddScoped<UpdateRaidTeamHandler>();
        services.AddScoped<DeleteRaidTeamHandler>();
        services.AddScoped<AddPlayerToRaidTeamHandler>();
        services.AddScoped<RemovePlayerFromRaidTeamHandler>();

        services.AddScoped<CreateRaidEventHandler>();
        services.AddScoped<GetRaidEventHandler>();
        services.AddScoped<ListRaidEventsHandler>();

        services.AddScoped<GetRaidTeamManagementOverviewHandler>();

        services.AddScoped<AddPlayerToGuildHandler>();
        services.AddScoped<AddAdditionalRoleToGuildMemberHandler>();
        services.AddScoped<RemoveAdditionalRoleFromGuildMemberHandler>();

        services.AddScoped<GetRosterOverviewHandler>();

        return services;
    }
}
