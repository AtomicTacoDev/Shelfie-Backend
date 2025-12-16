using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shelfie.Migrations
{
    /// <inheritdoc />
    public partial class AddBookshelfBook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookshelfBooks",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlacedObjectId = table.Column<int>(type: "integer", nullable: false),
                    UserBookId = table.Column<int>(type: "integer", nullable: false),
                    ShelfId = table.Column<string>(type: "text", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookshelfBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookshelfBooks_PlacedObjects_PlacedObjectId",
                        column: x => x.PlacedObjectId,
                        principalSchema: "app",
                        principalTable: "PlacedObjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookshelfBooks_UserBooks_UserBookId",
                        column: x => x.UserBookId,
                        principalSchema: "app",
                        principalTable: "UserBooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookshelfBooks_PlacedObjectId",
                schema: "app",
                table: "BookshelfBooks",
                column: "PlacedObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BookshelfBooks_UserBookId",
                schema: "app",
                table: "BookshelfBooks",
                column: "UserBookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookshelfBooks",
                schema: "app");
        }
    }
}
