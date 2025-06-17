using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserProfileId",
                table: "PlayerInventoryItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerInventoryItems_UserProfileId",
                table: "PlayerInventoryItems",
                column: "UserProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerInventoryItems_UserProfiles_UserProfileId",
                table: "PlayerInventoryItems",
                column: "UserProfileId",
                principalTable: "UserProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerInventoryItems_UserProfiles_UserProfileId",
                table: "PlayerInventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_PlayerInventoryItems_UserProfileId",
                table: "PlayerInventoryItems");

            migrationBuilder.DropColumn(
                name: "UserProfileId",
                table: "PlayerInventoryItems");
        }
    }
}
