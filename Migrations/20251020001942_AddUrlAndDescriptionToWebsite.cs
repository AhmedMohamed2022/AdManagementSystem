using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlAndDescriptionToWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "Url",
                table: "Websites");
        }
    }
}
