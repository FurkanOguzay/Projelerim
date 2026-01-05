using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpexTech.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    OrderId = table.Column<int>(type: "INTEGER", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Channel = table.Column<int>(type: "INTEGER", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BankName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AccountName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InstallmentCount = table.Column<int>(type: "INTEGER", nullable: true),
                    IsIncoming = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(5821));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(5827));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(5829));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(5830));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(5832));

            migrationBuilder.UpdateData(
                table: "AdminUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(7456));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(145));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(149));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(150));

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(152));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(9184));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(9189));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 393, DateTimeKind.Local).AddTicks(9191));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6820));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6826));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6828));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6847));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6849));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6851));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6853));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6856));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6857));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6859));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6861));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6863));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6865));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 14,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6866));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 15,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6868));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 16,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6870));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 17,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6872));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6874));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6876));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6877));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 21,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6879));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 22,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6881));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 23,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6883));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 24,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6885));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 25,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6887));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 26,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6888));

            migrationBuilder.UpdateData(
                table: "DeviceModels",
                keyColumn: "Id",
                keyValue: 27,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(6890));

            migrationBuilder.InsertData(
                table: "PriceLists",
                columns: new[] { "Id", "BasePriceListId", "CreatedAt", "Description", "DisplayOrder", "Factor", "IsActive", "Name", "Rounding", "UpdatedAt" },
                values: new object[] { 1, null, new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(9469), "Varsayılan fiyat listesi - 1x çarpan", 1, 1.00m, true, "Standart Liste", 0, null });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(4671));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(4681));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(4684));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(4687));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(4872));

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 394, DateTimeKind.Local).AddTicks(4877));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 23, 19, 34, 35, 392, DateTimeKind.Local).AddTicks(8701));

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserId",
                table: "Payments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DeleteData(
                table: "PriceLists",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4901));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4911));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4913));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4915));

            migrationBuilder.UpdateData(
                table: "AdminRoles",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(4945));

            migrationBuilder.UpdateData(
                table: "AdminUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2025, 12, 22, 18, 2, 49, 914, DateTimeKind.Local).AddTicks(7997));

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
        }
    }
}
