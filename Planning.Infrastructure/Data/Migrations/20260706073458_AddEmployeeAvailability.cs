using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planning.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeAvailabilities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DayPart = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeAvailabilities_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeAvailabilities_Users_LastModifiedByUserId",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeAvailabilities_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAvailabilities_LastModifiedByUserId",
                table: "EmployeeAvailabilities",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAvailabilities_OrganizationId",
                table: "EmployeeAvailabilities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAvailabilities_OrganizationId_UserId_Date",
                table: "EmployeeAvailabilities",
                columns: new[] { "OrganizationId", "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAvailabilities_OrganizationId_UserId_Date_Type_DayPart",
                table: "EmployeeAvailabilities",
                columns: new[] { "OrganizationId", "UserId", "Date", "Type", "DayPart" },
                unique: true,
                filter: "[Type] = 'DayPart'");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAvailabilities_UserId",
                table: "EmployeeAvailabilities",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeAvailabilities");
        }
    }
}
