using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreUserMangasConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowProgressForChaptersWithDecimals",
                table: "UserMangasConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SkipChapterToAnotherTranslation",
                table: "UserMangasConfigurations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowProgressForChaptersWithDecimals",
                table: "UserMangasConfigurations");

            migrationBuilder.DropColumn(
                name: "SkipChapterToAnotherTranslation",
                table: "UserMangasConfigurations");
        }
    }
}
