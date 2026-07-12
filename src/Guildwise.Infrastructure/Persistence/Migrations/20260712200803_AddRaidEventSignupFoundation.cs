using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guildwise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRaidEventSignupFoundation : Migration
    {
        private static readonly string[] RaidEventSignupUniqueIndexColumns = ["raid_event_id", "player_id"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "raid_event_signups",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    raid_event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_raid_event_signups", x => x.id);
                    table.ForeignKey(
                        name: "FK_raid_event_signups_players_player_id",
                        column: x => x.player_id,
                        principalTable: "players",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_raid_event_signups_raid_events_raid_event_id",
                        column: x => x.raid_event_id,
                        principalTable: "raid_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_raid_event_signups_player_id",
                table: "raid_event_signups",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "IX_raid_event_signups_raid_event_id_player_id",
                table: "raid_event_signups",
                columns: RaidEventSignupUniqueIndexColumns,
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "raid_event_signups");
        }
    }
}
