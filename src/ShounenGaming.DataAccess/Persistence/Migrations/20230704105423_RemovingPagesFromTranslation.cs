using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovingPagesFromTranslation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MangaUsersData_UserId",
                table: "MangaUsersData");

            migrationBuilder.DropColumn(
                name: "Pages",
                table: "MangaTranslations");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "MangaTranslations");

            migrationBuilder.AddColumn<bool>(
                name: "IsWorking",
                table: "MangaTranslations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_MangaUsersData_UserId_MangaId",
                table: "MangaUsersData",
                columns: new[] { "UserId", "MangaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_MangaUsersData_UserId_MangaId",
                table: "MangaUsersData");

            migrationBuilder.DropColumn(
                name: "IsWorking",
                table: "MangaTranslations");

            migrationBuilder.AddColumn<List<string>>(
                name: "Pages",
                table: "MangaTranslations",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "MangaTranslations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MangaUsersData_UserId",
                table: "MangaUsersData",
                column: "UserId");
        }
    }
}
