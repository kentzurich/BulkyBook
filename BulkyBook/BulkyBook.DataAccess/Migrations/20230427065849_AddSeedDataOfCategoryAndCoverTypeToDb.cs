using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedDataOfCategoryAndCoverTypeToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "DateTimeCreated", "DisplayOrder", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2023, 4, 27, 14, 58, 48, 832, DateTimeKind.Local).AddTicks(7215), 1, "Action" },
                    { 2, new DateTime(2023, 4, 27, 14, 58, 48, 832, DateTimeKind.Local).AddTicks(7218), 2, "Horror" },
                    { 3, new DateTime(2023, 4, 27, 14, 58, 48, 832, DateTimeKind.Local).AddTicks(7220), 3, "SciFi" }
                });

            migrationBuilder.InsertData(
                table: "CoverType",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Hard Cover" },
                    { 2, "Soft Cover" }
                });
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "CoverType",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "CoverType",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
