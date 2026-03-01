using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlanningSystem.DAL.Migrations
{
    /// <inheritdoc />
    [Migration("20250131120000_UpdateOrganizationRoles")]
    public partial class UpdateOrganizationRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Map old role values to new enum: Admin->Manager, Manager->Manager, Member->User, Viewer->User
            migrationBuilder.Sql(
                "UPDATE OrganizationUserMaps SET Role = 'Manager' WHERE Role = 'Admin'");
            migrationBuilder.Sql(
                "UPDATE OrganizationUserMaps SET Role = 'User' WHERE Role = 'Member'");
            migrationBuilder.Sql(
                "UPDATE OrganizationUserMaps SET Role = 'User' WHERE Role = 'Viewer'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert: Manager (that were Admin) -> Admin; User -> Member
            // Note: We cannot distinguish original Admin vs Manager, so all Manager stay Manager
            // User -> Member
            migrationBuilder.Sql(
                "UPDATE OrganizationUserMaps SET Role = 'Member' WHERE Role = 'User'");
            // Invited -> Member (Invited didn't exist before)
            migrationBuilder.Sql(
                "UPDATE OrganizationUserMaps SET Role = 'Member' WHERE Role = 'Invited'");
        }
    }
}
