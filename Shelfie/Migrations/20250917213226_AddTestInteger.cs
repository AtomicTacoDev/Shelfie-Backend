using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelfie.Migrations
{
    /// <inheritdoc />
    public partial class AddTestInteger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TestInteger",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TestInteger",
                table: "AspNetUsers");
        }
    }
}
