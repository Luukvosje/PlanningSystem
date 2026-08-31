using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresApproval",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // "Approved", not the generated "": every rule that already exists was entered before
            // approvals existed and is in force, so backfilling anything else would silently drop
            // live blockades out of the planning.
            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "AvailabilityRules",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Approved");

            migrationBuilder.AddColumn<DateTime>(
                name: "DecidedAtUtc",
                table: "AvailabilityRules",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DecidedByUserId",
                table: "AvailabilityRules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityRules_DecidedByUserId",
                table: "AvailabilityRules",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityRules_OrganizationId_ApprovalStatus",
                table: "AvailabilityRules",
                columns: new[] { "OrganizationId", "ApprovalStatus" });

            migrationBuilder.AddForeignKey(
                name: "FK_AvailabilityRules_Users_DecidedByUserId",
                table: "AvailabilityRules",
                column: "DecidedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AvailabilityRules_Users_DecidedByUserId",
                table: "AvailabilityRules");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilityRules_DecidedByUserId",
                table: "AvailabilityRules");

            migrationBuilder.DropIndex(
                name: "IX_AvailabilityRules_OrganizationId_ApprovalStatus",
                table: "AvailabilityRules");

            migrationBuilder.DropColumn(
                name: "RequiresApproval",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "AvailabilityRules");

            migrationBuilder.DropColumn(
                name: "DecidedAtUtc",
                table: "AvailabilityRules");

            migrationBuilder.DropColumn(
                name: "DecidedByUserId",
                table: "AvailabilityRules");
        }
    }
}
