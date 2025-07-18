using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ServerSettings",
                table: "ServerSettings");

            migrationBuilder.RenameTable(
                name: "ServerSettings",
                newName: "ProjectSettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProjectSettings",
                table: "ProjectSettings",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GlobalSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerTitle = table.Column<string>(type: "text", nullable: true),
                    ServerCopyright = table.Column<string>(type: "text", nullable: true),
                    GoogleClientId = table.Column<string>(type: "text", nullable: true),
                    ClickHouseConnection = table.Column<string>(type: "text", nullable: true),
                    GoogleClientSecret = table.Column<string>(type: "text", nullable: true),
                    GoogleServiceAccountJson = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalSettings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProjectSettings",
                table: "ProjectSettings");

            migrationBuilder.RenameTable(
                name: "ProjectSettings",
                newName: "ServerSettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServerSettings",
                table: "ServerSettings",
                column: "Id");
        }
    }
}

