using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBundleIdToProjectSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BundleId",
                table: "ProjectSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BundleId",
                table: "ProjectSettings");
        }
    }
}
