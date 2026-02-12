using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CRM.Migrations
{
    /// <inheritdoc />
    public partial class AddArchivedAtColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_SalesRepId",
                table: "Customers",
                column: "SalesRepId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_AspNetUsers_SalesRepId",
                table: "Customers",
                column: "SalesRepId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_AspNetUsers_SalesRepId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_SalesRepId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "Customers");
        }
    }
}
