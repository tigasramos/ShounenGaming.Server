using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DownloadedField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldSaveImages",
                table: "Mangas");

            migrationBuilder.AddColumn<bool>(
                name: "Downloaded",
                table: "MangaTranslations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Downloaded",
                table: "MangaTranslations");

            migrationBuilder.AddColumn<bool>(
                name: "ShouldSaveImages",
                table: "Mangas",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
