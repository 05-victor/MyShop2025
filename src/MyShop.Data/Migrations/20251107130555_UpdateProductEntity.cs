using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyShop.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "count",
                table: "products",
                newName: "selling_price");

            migrationBuilder.AddColumn<double>(
                name: "commission_rate",
                table: "products",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "device_type",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_url",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "manufacturer",
                table: "products",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "products",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "commission_rate",
                table: "products");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "products");

            migrationBuilder.DropColumn(
                name: "device_type",
                table: "products");

            migrationBuilder.DropColumn(
                name: "image_url",
                table: "products");

            migrationBuilder.DropColumn(
                name: "manufacturer",
                table: "products");

            migrationBuilder.DropColumn(
                name: "quantity",
                table: "products");

            migrationBuilder.DropColumn(
                name: "status",
                table: "products");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "selling_price",
                table: "products",
                newName: "count");
        }
    }
}
