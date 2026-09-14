using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Planning.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Customers",
                type: "character varying(7)",
                maxLength: 7,
                nullable: false,
                defaultValue: "#6366F1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Customers");
        }
    }
}
