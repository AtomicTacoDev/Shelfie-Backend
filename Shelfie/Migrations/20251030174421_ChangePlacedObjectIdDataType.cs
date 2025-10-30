using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelfie.Migrations
{
    /// <inheritdoc />
    public partial class ChangePlacedObjectIdDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectId",
                schema: "app",
                table: "PlacedObjects");

            migrationBuilder.AddColumn<string>(
                name: "ObjectTypeId",
                schema: "app",
                table: "PlacedObjects",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ObjectTypeId",
                schema: "app",
                table: "PlacedObjects");

            migrationBuilder.AddColumn<int>(
                name: "ObjectId",
                schema: "app",
                table: "PlacedObjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
