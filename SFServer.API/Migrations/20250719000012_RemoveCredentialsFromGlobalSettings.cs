using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCredentialsFromGlobalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClickHouseConnection",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "GoogleClientId",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "GoogleClientSecret",
                table: "GlobalSettings");

            migrationBuilder.DropColumn(
                name: "GoogleServiceAccountJson",
                table: "GlobalSettings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClickHouseConnection",
                table: "GlobalSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleClientId",
                table: "GlobalSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleClientSecret",
                table: "GlobalSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleServiceAccountJson",
                table: "GlobalSettings",
                type: "text",
                nullable: true);
        }
    }
}
