using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelfie.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                schema: "app",
                table: "UserBooks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CoverUrl",
                schema: "app",
                table: "UserBooks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "app",
                table: "UserBooks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                schema: "app",
                table: "UserBooks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PublishedDate",
                schema: "app",
                table: "UserBooks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Rating",
                schema: "app",
                table: "UserBooks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Title",
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
                name: "Author",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "CoverUrl",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "PageCount",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "Rating",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "Title",
                schema: "app",
                table: "UserBooks");
        }
    }
}
