using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "S3Key",
                table: "Configs");

            migrationBuilder.CreateTable(
                name: "ConfigFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: true),
                    S3Key = table.Column<string>(type: "text", nullable: true),
                    ConfigMetadataId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigFiles_Configs_ConfigMetadataId",
                        column: x => x.ConfigMetadataId,
                        principalTable: "Configs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfigFiles_ConfigMetadataId",
                table: "ConfigFiles",
                column: "ConfigMetadataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigFiles");

            migrationBuilder.AddColumn<string>(
                name: "S3Key",
                table: "Configs",
                type: "text",
                nullable: true);
        }
    }
}
