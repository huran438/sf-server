using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserProfilePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "UserProfiles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "UserProfiles",
                type: "text",
                nullable: true);
        }
    }
}
