using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzastaAdminBackend.Migrations
{
    /// <inheritdoc />
    public partial class changedCustomerOrderAddedUserData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrderBy",
                table: "CustomOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderByNumber",
                table: "CustomOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderBy",
                table: "CustomOrders");

            migrationBuilder.DropColumn(
                name: "OrderByNumber",
                table: "CustomOrders");
        }
    }
}
