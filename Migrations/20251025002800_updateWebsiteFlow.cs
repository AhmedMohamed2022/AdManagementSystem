using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class updateWebsiteFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_Websites_WebsiteId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Websites_AspNetUsers_PublisherId",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Ads_WebsiteId",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "WebsiteId",
                table: "Ads");

            migrationBuilder.RenameColumn(
                name: "PublisherId",
                table: "Websites",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Websites_PublisherId",
                table: "Websites",
                newName: "IX_Websites_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ScriptKey",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Websites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "WebsiteId",
                table: "Impressions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WebsiteId",
                table: "Clicks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Impressions_WebsiteId",
                table: "Impressions",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Clicks_WebsiteId",
                table: "Clicks",
                column: "WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clicks_Websites_WebsiteId",
                table: "Clicks",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Impressions_Websites_WebsiteId",
                table: "Impressions",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_AspNetUsers_UserId",
                table: "Websites",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clicks_Websites_WebsiteId",
                table: "Clicks");

            migrationBuilder.DropForeignKey(
                name: "FK_Impressions_Websites_WebsiteId",
                table: "Impressions");

            migrationBuilder.DropForeignKey(
                name: "FK_Websites_AspNetUsers_UserId",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Impressions_WebsiteId",
                table: "Impressions");

            migrationBuilder.DropIndex(
                name: "IX_Clicks_WebsiteId",
                table: "Clicks");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "WebsiteId",
                table: "Impressions");

            migrationBuilder.DropColumn(
                name: "WebsiteId",
                table: "Clicks");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Websites",
                newName: "PublisherId");

            migrationBuilder.RenameIndex(
                name: "IX_Websites_UserId",
                table: "Websites",
                newName: "IX_Websites_PublisherId");

            migrationBuilder.AlterColumn<string>(
                name: "Url",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ScriptKey",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "WebsiteId",
                table: "Ads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ads_WebsiteId",
                table: "Ads",
                column: "WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_Websites_WebsiteId",
                table: "Ads",
                column: "WebsiteId",
                principalTable: "Websites",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_AspNetUsers_PublisherId",
                table: "Websites",
                column: "PublisherId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
