using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861

namespace Guildwise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialGuildRosterPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "guilds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    realm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guilds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "raid_teams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raid_teams", x => x.id);
                    table.ForeignKey(
                        name: "FK_raid_teams_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "characters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    region = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    realm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    character_class = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    character_specialization = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    character_role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_characters", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    main_character_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_players", x => x.id);
                    table.ForeignKey(
                        name: "FK_players_characters_main_character_id",
                        column: x => x.main_character_id,
                        principalTable: "characters",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "guild_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_rank = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    additional_roles = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guild_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_guild_members_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_guild_members_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "raid_team_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    raid_team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raid_team_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_raid_team_members_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_raid_team_members_raid_teams_raid_team_id",
                        column: x => x.raid_team_id,
                        principalTable: "raid_teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_characters_player_id_region_realm_name",
                table: "characters",
                columns: new[] { "player_id", "region", "realm", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_members_guild_id_player_id",
                table: "guild_members",
                columns: new[] { "guild_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_guild_members_player_id",
                table: "guild_members",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_main_character_id",
                table: "players",
                column: "main_character_id");

            migrationBuilder.CreateIndex(
                name: "IX_raid_team_members_player_id",
                table: "raid_team_members",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_raid_team_members_raid_team_id_player_id",
                table: "raid_team_members",
                columns: new[] { "raid_team_id", "player_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_raid_teams_guild_id_name",
                table: "raid_teams",
                columns: new[] { "guild_id", "name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_characters_players_player_id",
                table: "characters",
                column: "player_id",
                principalTable: "players",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_characters_players_player_id",
                table: "characters");

            migrationBuilder.DropTable(
                name: "guild_members");

            migrationBuilder.DropTable(
                name: "raid_team_members");

            migrationBuilder.DropTable(
                name: "raid_teams");

            migrationBuilder.DropTable(
                name: "guilds");

            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "characters");
        }
    }
}

#pragma warning restore CA1861
