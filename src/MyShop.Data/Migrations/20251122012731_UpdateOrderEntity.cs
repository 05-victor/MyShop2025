using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "final_price",
                table: "orders",
                newName: "total_amount");

            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "discount_amount",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "grand_total",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_status",
                table: "orders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "sale_agent_id",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "shipping_fee",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "tax_amount",
                table: "orders",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_sale_agent_id",
                table: "orders",
                column: "sale_agent_id");

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_customer_id",
                table: "orders",
                column: "customer_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_orders_users_sale_agent_id",
                table: "orders",
                column: "sale_agent_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_customer_id",
                table: "orders");

            migrationBuilder.DropForeignKey(
                name: "fk_orders_users_sale_agent_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_customer_id",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "ix_orders_sale_agent_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "customer_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "discount_amount",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "grand_total",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "note",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "payment_status",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "sale_agent_id",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "shipping_fee",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "tax_amount",
                table: "orders");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "orders",
                newName: "final_price");
        }
    }
}
