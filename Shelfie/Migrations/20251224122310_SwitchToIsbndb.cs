using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Shelfie.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToIsbndb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EditionOlid",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "WorkOlid",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                schema: "app",
                table: "UserBooks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Books",
                schema: "app",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Isbn13 = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    Isbn10 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Isbn = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: true),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Author = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Synopsis = table.Column<string>(type: "text", nullable: true),
                    CoverImage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DatePublished = table.Column<string>(type: "text", nullable: true),
                    PageCount = table.Column<int>(type: "integer", nullable: true),
                    HeightInches = table.Column<decimal>(type: "numeric", nullable: true),
                    WidthInches = table.Column<decimal>(type: "numeric", nullable: true),
                    LengthInches = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserBooks_BookId",
                schema: "app",
                table: "UserBooks",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Isbn",
                schema: "app",
                table: "Books",
                column: "Isbn");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Isbn10",
                schema: "app",
                table: "Books",
                column: "Isbn10");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Isbn13",
                schema: "app",
                table: "Books",
                column: "Isbn13");

            migrationBuilder.AddForeignKey(
                name: "FK_UserBooks_Books_BookId",
                schema: "app",
                table: "UserBooks",
                column: "BookId",
                principalSchema: "app",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserBooks_Books_BookId",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropTable(
                name: "Books",
                schema: "app");

            migrationBuilder.DropIndex(
                name: "IX_UserBooks_BookId",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.DropColumn(
                name: "BookId",
                schema: "app",
                table: "UserBooks");

            migrationBuilder.AddColumn<string>(
                name: "EditionOlid",
                schema: "app",
                table: "UserBooks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WorkOlid",
                schema: "app",
                table: "UserBooks",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
