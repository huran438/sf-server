using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServerSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServerSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServerCopyright = table.Column<string>(type: "text", nullable: true),
                    GoogleClientId = table.Column<string>(type: "text", nullable: true),
                    GoogleClientSecret = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerSettings");
        }
    }
}
