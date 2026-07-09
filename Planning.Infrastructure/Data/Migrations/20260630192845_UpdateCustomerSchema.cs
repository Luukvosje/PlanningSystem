using Microsoft.EntityFrameworkCore.Migrations;
using Planning.Domain.Customers;

namespace Planning.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningRecords_Customers_CustomerId",
                table: "PlanningRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Customers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Customers");

            migrationBuilder.RenameTable(
                name: "Customers",
                newName: "customer");

            migrationBuilder.RenameIndex(
                name: "IX_Customers_OrganizationId",
                table: "customer",
                newName: "IX_customer_OrganizationId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "customer",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_customer",
                table: "customer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_customer_Organizations_OrganizationId",
                table: "customer",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningRecords_customer_CustomerId",
                table: "PlanningRecords",
                column: "CustomerId",
                principalTable: "customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_customer_Organizations_OrganizationId",
                table: "customer");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanningRecords_customer_CustomerId",
                table: "PlanningRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_customer",
                table: "customer");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "customer");

            migrationBuilder.RenameTable(
                name: "customer",
                newName: "Customers");

            migrationBuilder.RenameIndex(
                name: "IX_customer_OrganizationId",
                table: "Customers",
                newName: "IX_Customers_OrganizationId");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Customers",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Customers",
                table: "Customers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Organizations_OrganizationId",
                table: "Customers",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlanningRecords_Customers_CustomerId",
                table: "PlanningRecords",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
