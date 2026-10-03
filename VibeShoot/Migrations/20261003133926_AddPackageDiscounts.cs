using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VibeShoot.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageDiscounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEnd",
                table: "Packages",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountLabel",
                table: "Packages",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountStart",
                table: "Packages",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "Packages",
                type: "varchar(16)",
                maxLength: 16,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "Packages",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Bookings",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "DiscountLabel",
                table: "Bookings",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Bookings",
                type: "decimal(12,2)",
                nullable: false,
                defaultValue: 0m);

            // Bookings made before discounts existed were charged the regular price.
            migrationBuilder.Sql("UPDATE `Bookings` SET `OriginalPrice` = `TotalPrice`;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEnd",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DiscountLabel",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DiscountStart",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "Packages");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountLabel",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Bookings");
        }
    }
}
