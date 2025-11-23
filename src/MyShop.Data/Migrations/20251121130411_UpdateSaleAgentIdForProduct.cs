using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSaleAgentIdForProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "sale_agent_id",
                table: "products",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "order_date",
                table: "orders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_products_sale_agent_id",
                table: "products",
                column: "sale_agent_id");

            migrationBuilder.AddForeignKey(
                name: "fk_products_users_sale_agent_id",
                table: "products",
                column: "sale_agent_id",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_products_users_sale_agent_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "ix_products_sale_agent_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "sale_agent_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "order_date",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "status",
                table: "orders");
        }
    }
}
