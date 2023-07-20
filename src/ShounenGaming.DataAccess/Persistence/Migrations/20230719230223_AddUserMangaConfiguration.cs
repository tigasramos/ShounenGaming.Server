using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMangaConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNSFW",
                table: "Mangas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "UserMangasConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReadingMode = table.Column<int>(type: "integer", nullable: false),
                    NSFWBehaviour = table.Column<int>(type: "integer", nullable: false),
                    TranslationLanguage = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserMangasConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserMangasConfigurations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserMangasConfigurations_UserId",
                table: "UserMangasConfigurations",
                column: "UserId",
                unique: true);

            // This Script will Create Manga Configurations for already existing Users
            migrationBuilder.Sql("INSERT INTO public.\"UserMangasConfigurations\" (\"ReadingMode\", \"NSFWBehaviour\", \"TranslationLanguage\", \"UserId\") SELECT 3 AS \"ReadingMode\", 0 AS \"NSFWBehaviour\", 0 AS \"TranslationLanguage\", u.\"Id\" AS \"UserId\" FROM public.\"Users\" u;");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserMangasConfigurations");

            migrationBuilder.DropColumn(
                name: "IsNSFW",
                table: "Mangas");
        }
    }
}
