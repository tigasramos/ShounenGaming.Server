using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ALScore",
                table: "Mangas",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MALScore",
                table: "Mangas",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ALScore",
                table: "Mangas");

            migrationBuilder.DropColumn(
                name: "MALScore",
                table: "Mangas");
        }
    }
}
