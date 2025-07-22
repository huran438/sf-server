using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SFServer.API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure all products have a ProductId
            migrationBuilder.Sql(@"UPDATE ""Products"" SET ""ProductId"" = lower(replace(""Id""::text,'-','')) WHERE ""ProductId"" IS NULL;");

            // Remove duplicates before creating the unique index
            migrationBuilder.Sql(@"DELETE FROM ""Products"" a USING ""Products"" b WHERE a.ctid < b.ctid AND a.""ProjectId"" = b.""ProjectId"" AND a.""ProductId"" IS NOT DISTINCT FROM b.""ProductId"";");

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProjectId_ProductId",
                table: "Products",
                columns: new[] { "ProjectId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_ProjectId_ProductId",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "Products",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
