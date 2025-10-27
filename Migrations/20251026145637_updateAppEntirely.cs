using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AdManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class updateAppEntirely : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Websites_AspNetUsers_UserId",
                table: "Websites");

            migrationBuilder.DropTable(
                name: "Clicks");

            migrationBuilder.DropTable(
                name: "Impressions");

            migrationBuilder.DropColumn(
                name: "Clicks",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Ads");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Websites",
                newName: "ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "Websites",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Websites_UserId",
                table: "Websites",
                newName: "IX_Websites_ApplicationUserId");

            migrationBuilder.RenameColumn(
                name: "Impressions",
                table: "Ads",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Ads",
                newName: "StartDate");

            migrationBuilder.AlterColumn<string>(
                name: "ScriptKey",
                table: "Websites",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Websites",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Websites",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Websites",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Ads",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TargetUrl",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AdvertiserId",
                table: "Ads",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Ads",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Ads",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Ads",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Ads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxClicks",
                table: "Ads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxImpressions",
                table: "Ads",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AdClicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdId = table.Column<int>(type: "int", nullable: false),
                    HostDomain = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ClickedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdClicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdClicks_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdImpressions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdId = table.Column<int>(type: "int", nullable: false),
                    HostDomain = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IPAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdImpressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdImpressions_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Websites_Domain",
                table: "Websites",
                column: "Domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Websites_ScriptKey",
                table: "Websites",
                column: "ScriptKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ads_ApplicationUserId",
                table: "Ads",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Ads_Status",
                table: "Ads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AdClicks_AdId",
                table: "AdClicks",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_AdImpressions_AdId",
                table: "AdImpressions",
                column: "AdId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ads_AspNetUsers_ApplicationUserId",
                table: "Ads",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_AspNetUsers_ApplicationUserId",
                table: "Websites",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ads_AspNetUsers_ApplicationUserId",
                table: "Ads");

            migrationBuilder.DropForeignKey(
                name: "FK_Websites_AspNetUsers_ApplicationUserId",
                table: "Websites");

            migrationBuilder.DropTable(
                name: "AdClicks");

            migrationBuilder.DropTable(
                name: "AdImpressions");

            migrationBuilder.DropIndex(
                name: "IX_Websites_Domain",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Websites_ScriptKey",
                table: "Websites");

            migrationBuilder.DropIndex(
                name: "IX_Ads_ApplicationUserId",
                table: "Ads");

            migrationBuilder.DropIndex(
                name: "IX_Ads_Status",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Websites");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "MaxClicks",
                table: "Ads");

            migrationBuilder.DropColumn(
                name: "MaxImpressions",
                table: "Ads");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "Websites",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "Websites",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Websites_ApplicationUserId",
                table: "Websites",
                newName: "IX_Websites_UserId");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Ads",
                newName: "Impressions");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Ads",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "ScriptKey",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Websites",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "TargetUrl",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Ads",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AdvertiserId",
                table: "Ads",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "Clicks",
                table: "Ads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Ads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Clicks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdId = table.Column<int>(type: "int", nullable: false),
                    WebsiteId = table.Column<int>(type: "int", nullable: false),
                    ClickerIp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clicks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clicks_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Clicks_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Impressions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdId = table.Column<int>(type: "int", nullable: false),
                    WebsiteId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViewerIp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Impressions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Impressions_Ads_AdId",
                        column: x => x.AdId,
                        principalTable: "Ads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Impressions_Websites_WebsiteId",
                        column: x => x.WebsiteId,
                        principalTable: "Websites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clicks_AdId",
                table: "Clicks",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_Clicks_WebsiteId",
                table: "Clicks",
                column: "WebsiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Impressions_AdId",
                table: "Impressions",
                column: "AdId");

            migrationBuilder.CreateIndex(
                name: "IX_Impressions_WebsiteId",
                table: "Impressions",
                column: "WebsiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Websites_AspNetUsers_UserId",
                table: "Websites",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
