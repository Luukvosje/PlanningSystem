using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationPlanningSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportantWorkTimes",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[\"06:00:00\",\"09:00:00\",\"13:00:00\",\"17:00:00\",\"21:00:00\"]");

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportantWorkTimes",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "Organizations");
        }
    }
}
