using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedResumeCounterForUserSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaused",
                table: "UserSessions");

            migrationBuilder.AddColumn<long>(
                name: "ResumeCounter",
                table: "UserSessions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumeCounter",
                table: "UserSessions");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "UserSessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
