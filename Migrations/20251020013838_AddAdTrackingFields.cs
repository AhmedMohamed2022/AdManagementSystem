using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddAdTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "Ads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Impressions",
                table: "Ads",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Clicks",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Impressions",
                table: "Ads");
        }
    }
}
