using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace UpexTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminRolesAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsSystemRole = table.Column<bool>(type: "INTEGER", nullable: false),
                    Permissions = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RoleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdminUsers_AdminRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AdminRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AdminRoles",
                columns: new[] { "Id", "CreatedAt", "Description", "IsActive", "IsSystemRole", "Name", "Permissions", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4901), "Tüm sistem yetkilerine sahip en üst düzey yönetici", true, true, "Süper Admin", 2147483647, null },
                    { 2, new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4911), "Finans, muhasebe ve cari hesap işlemleri", true, false, "Muhasebeci", 12417, null },
                    { 3, new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4913), "Ürün ve envanter yönetimi", true, false, "Stokçu", 121, null },
                    { 4, new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4915), "Müşteri, sipariş ve iade işlemleri", true, false, "Müşteri Temsilcisi", 1799, null },
                    { 5, new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4945), "Banner ve kampanya yönetimi", true, false, "Pazarlamacı", 22529, null }
                });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(2058));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(2067));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(2069));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(2070));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(470));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(481));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(483));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2387));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2397));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2400));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2401));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2403));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2405));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2407));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2409));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2410));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2440));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2442));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2444));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2446));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2449));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2467));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2469));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2470));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2473));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2475));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2478));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2480));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2482));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2484));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2486));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2488));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2489));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 916, DateTimeKind.Local).AddTicks(2491));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(9338));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(9353));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(9357));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(9388));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(9741));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 915, DateTimeKind.Local).AddTicks(9754));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 913, DateTimeKind.Local).AddTicks(5603));

            migrationBuilder.InsertData(
                table: "AdminUsers",
                columns: new[] { "Id", "CreatedAt", "Email", "FirstName", "IsActive", "LastLoginAt", "LastName", "PasswordHash", "Phone", "RoleId", "UpdatedAt" },
                values: new object[] { 1, new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(7997), "admin@upextech.com", "Süper", true, null, "Admin", "admin123", null, 1, null });

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_Email",
                table: "AdminUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminUsers_RoleId",
                table: "AdminUsers",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "AdminRoles");

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 566, DateTimeKind.Local).AddTicks(9309));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 566, DateTimeKind.Local).AddTicks(9315));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 566, DateTimeKind.Local).AddTicks(9318));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 566, DateTimeKind.Local).AddTicks(9320));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 566, DateTimeKind.Local).AddTicks(8043));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 566, DateTimeKind.Local).AddTicks(8050));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 566, DateTimeKind.Local).AddTicks(8053));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7508));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7517));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7537));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7540));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7542));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7544));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7546));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7548));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7551));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7553));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7555));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7557));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7559));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7572));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7574));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7576));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7578));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7580));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7582));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7584));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7586));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7588));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7590));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7592));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7594));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7596));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(7598));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(5139));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(5152));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(5157));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(5161));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(5411));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 567, DateTimeKind.Local).AddTicks(5421));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 15, 39, 35, 565, DateTimeKind.Local).AddTicks(8537));
        }
    }
}
