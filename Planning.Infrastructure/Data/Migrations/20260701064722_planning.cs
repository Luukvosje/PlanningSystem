using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Planning.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class planning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "PlanningRecords",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "PlanningRecords",
                type: "nvarchar(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#6366F1");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PlanningRecords",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlanningRecords_OrganizationId_StartUtc_EndUtc",
                table: "PlanningRecords",
                columns: new[] { "OrganizationId", "StartUtc", "EndUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlanningRecords_OrganizationId_StartUtc_EndUtc",
                table: "PlanningRecords");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "PlanningRecords");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PlanningRecords");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                table: "PlanningRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
