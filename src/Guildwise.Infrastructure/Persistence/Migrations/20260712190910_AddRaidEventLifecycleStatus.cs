using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guildwise.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRaidEventLifecycleStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "raid_events",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Scheduled");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "raid_events");
        }
    }
}
