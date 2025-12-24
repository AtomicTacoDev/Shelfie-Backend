using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelfie.Migrations
{
    /// <inheritdoc />
    public partial class ChangeGoogleBooksToOL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GoogleBooksId",
                schema: "app",
                table: "UserBooks",
                newName: "WorkOlid");

            migrationBuilder.AddColumn<string>(
                name: "EditionOlid",
                schema: "app",
                table: "UserBooks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditionOlid",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.RenameColumn(
                name: "WorkOlid",
                schema: "app",
                table: "UserBooks",
                newName: "GoogleBooksId");
        }
    }
}
