using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzastaAdminBackend.Migrations
{
    /// <inheritdoc />
    public partial class addedOrdersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Crust = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sauce = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Toppings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderQuantity = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomOrders", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomOrders");
        }
    }
}
