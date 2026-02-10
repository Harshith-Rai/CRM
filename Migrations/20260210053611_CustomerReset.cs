using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Migrations
{
    /// <inheritdoc />
    public partial class CustomerReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Customers",
                newName: "SalesRepId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Customers",
                newName: "Industry");

            migrationBuilder.RenameColumn(
                name: "Company",
                table: "Customers",
                newName: "CompanyName");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Customers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "SalesRepId",
                table: "Customers",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Industry",
                table: "Customers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "Customers",
                newName: "Company");
        }
    }
}
