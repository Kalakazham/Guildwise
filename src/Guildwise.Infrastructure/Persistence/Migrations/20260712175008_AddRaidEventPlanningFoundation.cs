using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guildwise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRaidEventPlanningFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "raid_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    guild_id = table.Column<Guid>(type: "uuid", nullable: false),
                    raid_team_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    instance_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    difficulty = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raid_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_raid_events_guilds_guild_id",
                        column: x => x.guild_id,
                        principalTable: "guilds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_raid_events_raid_teams_raid_team_id",
                        column: x => x.raid_team_id,
                        principalTable: "raid_teams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_raid_events_guild_id",
                table: "raid_events",
                column: "guild_id");

            migrationBuilder.CreateIndex(
                name: "IX_raid_events_raid_team_id",
                table: "raid_events",
                column: "raid_team_id");

            migrationBuilder.CreateIndex(
                name: "IX_raid_events_start_time",
                table: "raid_events",
                column: "start_time");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "raid_events");
        }
    }
}
