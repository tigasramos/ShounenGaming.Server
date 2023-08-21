using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRatingToMangas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "MangaUsersData",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rating",
                table: "MangaUsersData");
        }
    }
}
