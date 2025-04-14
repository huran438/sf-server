using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    DeviceModel = table.Column<string>(type: "text", nullable: true),
                    DeviceName = table.Column<string>(type: "text", nullable: true),
                    DeviceType = table.Column<string>(type: "text", nullable: true),
                    OperatingSystem = table.Column<string>(type: "text", nullable: true),
                    ProcessorType = table.Column<string>(type: "text", nullable: true),
                    ProcessorCount = table.Column<int>(type: "integer", nullable: false),
                    SystemMemorySize = table.Column<int>(type: "integer", nullable: false),
                    GraphicsDeviceName = table.Column<string>(type: "text", nullable: true),
                    GraphicsDeviceVendor = table.Column<string>(type: "text", nullable: true),
                    GraphicsMemorySize = table.Column<int>(type: "integer", nullable: false),
                    GraphicsDeviceVersion = table.Column<string>(type: "text", nullable: true),
                    GraphicsDeviceType = table.Column<string>(type: "text", nullable: true),
                    GraphicsShaderLevel = table.Column<string>(type: "text", nullable: true),
                    ScreenWidth = table.Column<int>(type: "integer", nullable: false),
                    ScreenHeight = table.Column<int>(type: "integer", nullable: false),
                    ScreenDpi = table.Column<float>(type: "real", nullable: false),
                    FullScreen = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevices", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDevices");
        }
    }
}
