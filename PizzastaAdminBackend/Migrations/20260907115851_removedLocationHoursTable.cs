using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzastaAdminBackend.Migrations
{
    /// <inheritdoc />
    public partial class removedLocationHoursTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationOpeningHours");

            migrationBuilder.AddColumn<string>(
                name: "OpeningHours",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningHours",
                table: "Locations");

            migrationBuilder.CreateTable(
                name: "LocationOpeningHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Day = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Hours = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationOpeningHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationOpeningHours_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationOpeningHours_LocationId_Day",
                table: "LocationOpeningHours",
                columns: new[] { "LocationId", "Day" },
                unique: true);
        }
    }
}
