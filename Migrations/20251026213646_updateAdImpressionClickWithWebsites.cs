using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class updateAdImpressionClickWithWebsites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WebsiteId",
                table: "AdImpressions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WebsiteId",
                table: "AdClicks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AdImpressions_WebsiteId",
                table: "AdImpressions",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_AdClicks_WebsiteId",
                table: "AdClicks",
                column: "WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdClicks_Websites_WebsiteId",
                table: "AdClicks",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdImpressions_Websites_WebsiteId",
                table: "AdImpressions",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdClicks_Websites_WebsiteId",
                table: "AdClicks");

            migrationBuilder.DropForeignKey(
                name: "FK_AdImpressions_Websites_WebsiteId",
                table: "AdImpressions");

            migrationBuilder.DropIndex(
                name: "IX_AdImpressions_WebsiteId",
                table: "AdImpressions");

            migrationBuilder.DropIndex(
                name: "IX_AdClicks_WebsiteId",
                table: "AdClicks");

            migrationBuilder.DropColumn(
                name: "WebsiteId",
                table: "AdImpressions");

            migrationBuilder.DropColumn(
                name: "WebsiteId",
                table: "AdClicks");
        }
    }
}
