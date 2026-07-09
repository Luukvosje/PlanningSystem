using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Planning.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAppModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganizationModules",
                columns: table => new
                {
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationModules", x => new { x.OrganizationId, x.Module });
                    table.ForeignKey(
                        name: "FK_OrganizationModules_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserModules",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserModules", x => new { x.UserId, x.Module });
                    table.ForeignKey(
                        name: "FK_UserModules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationModules_OrganizationId",
                table: "OrganizationModules",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserModules_UserId",
                table: "UserModules",
                column: "UserId");

            migrationBuilder.Sql("""
                INSERT INTO OrganizationModules (OrganizationId, Module, IsEnabled)
                SELECT o.Id, m.Module, CAST(1 AS bit)
                FROM Organizations o
                CROSS JOIN (VALUES ('Planning'), ('Klant'), ('Beheer')) AS m(Module);

                INSERT INTO UserModules (UserId, Module, IsEnabled)
                SELECT u.Id, m.Module, CAST(1 AS bit)
                FROM Users u
                CROSS JOIN (VALUES ('Planning'), ('Klant'), ('Beheer')) AS m(Module);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganizationModules");

            migrationBuilder.DropTable(
                name: "UserModules");
        }
    }
}
