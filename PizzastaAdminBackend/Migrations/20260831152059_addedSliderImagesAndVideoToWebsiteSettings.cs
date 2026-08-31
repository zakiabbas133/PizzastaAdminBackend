using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PizzastaAdminBackend.Migrations
{
    /// <inheritdoc />
    public partial class addedSliderImagesAndVideoToWebsiteSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SliderImages",
                table: "WebsiteSettings",
                type: "nvarchar(max)",
                maxLength: 5000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Video",
                table: "WebsiteSettings",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SliderImages",
                table: "WebsiteSettings");

            migrationBuilder.DropColumn(
                name: "Video",
                table: "WebsiteSettings");
        }
    }
}
