using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class BasicInventorySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "UserProfileIndex",
                schema: "dbo");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "UserProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsAvailableToBuy = table.Column<bool>(type: "boolean", nullable: false),
                    IsAvailableToDrop = table.Column<bool>(type: "boolean", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlayerInventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerInventoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerInventoryItems_InventoryItems_ItemId",
                        column: x => x.ItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerInventoryItems_UserProfiles_UserId",
                        column: x => x.UserId,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventoryItems_ItemId",
                table: "PlayerInventoryItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventoryItems_UserId",
                table: "PlayerInventoryItems",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerInventoryItems");

            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "UserProfiles");

            migrationBuilder.DropSequence(
                name: "UserProfileIndex",
                schema: "dbo");
        }
    }
}
