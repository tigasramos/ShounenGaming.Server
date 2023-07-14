using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MangaTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaTags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MangaWriters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaWriters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiscordId = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mangas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsReleasing = table.Column<bool>(type: "boolean", nullable: false),
                    ImagesUrls = table.Column<List<string>>(type: "text[]", nullable: false),
                    WriterId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ShouldSaveImages = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    MangaMyAnimeListID = table.Column<long>(type: "bigint", nullable: true),
                    MALPopularity = table.Column<int>(type: "integer", nullable: true),
                    MangaAniListID = table.Column<long>(type: "bigint", nullable: true),
                    ALPopularity = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mangas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mangas_MangaWriters_WriterId",
                        column: x => x.WriterId,
                        principalTable: "MangaWriters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    DiscordVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Birthday = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ServerMemberId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_ServerMembers_ServerMemberId",
                        column: x => x.ServerMemberId,
                        principalTable: "ServerMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MangaAlternativeNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Language = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MangaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaAlternativeNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaAlternativeNames_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaChapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<double>(type: "double precision", nullable: false),
                    MangaId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaChapters_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaMangaTag",
                columns: table => new
                {
                    MangasId = table.Column<int>(type: "integer", nullable: false),
                    TagsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaMangaTag", x => new { x.MangasId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_MangaMangaTag_MangaTags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "MangaTags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MangaMangaTag_Mangas_MangasId",
                        column: x => x.MangasId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ImageURL = table.Column<string>(type: "text", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: false),
                    MangaId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaSources_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaSynonym",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    MangaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaSynonym", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaSynonym_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AddedMangaHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MangaId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddedMangaHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddedMangaHistory_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddedMangaHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PreviousState = table.Column<int>(type: "integer", nullable: true),
                    NewState = table.Column<int>(type: "integer", nullable: true),
                    MangaId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaStatusHistory_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MangaStatusHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaUsersData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MangaId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaUsersData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaUsersData_Mangas_MangaId",
                        column: x => x.MangaId,
                        principalTable: "Mangas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MangaUsersData_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaChaptersHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChapterId = table.Column<int>(type: "integer", nullable: false),
                    Read = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaChaptersHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaChaptersHistory_MangaChapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "MangaChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MangaChaptersHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Language = table.Column<int>(type: "integer", nullable: false),
                    ReleasedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Provider = table.Column<string>(type: "text", nullable: false),
                    Pages = table.Column<List<string>>(type: "text[]", nullable: false),
                    MangaChapterId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MangaTranslations_MangaChapters_MangaChapterId",
                        column: x => x.MangaChapterId,
                        principalTable: "MangaChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MangaChapterMangaUserData",
                columns: table => new
                {
                    ChaptersReadId = table.Column<int>(type: "integer", nullable: false),
                    MangaUserDataId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MangaChapterMangaUserData", x => new { x.ChaptersReadId, x.MangaUserDataId });
                    table.ForeignKey(
                        name: "FK_MangaChapterMangaUserData_MangaChapters_ChaptersReadId",
                        column: x => x.ChaptersReadId,
                        principalTable: "MangaChapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MangaChapterMangaUserData_MangaUsersData_MangaUserDataId",
                        column: x => x.MangaUserDataId,
                        principalTable: "MangaUsersData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddedMangaHistory_MangaId",
                table: "AddedMangaHistory",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_AddedMangaHistory_UserId",
                table: "AddedMangaHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaAlternativeNames_MangaId",
                table: "MangaAlternativeNames",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaChapterMangaUserData_MangaUserDataId",
                table: "MangaChapterMangaUserData",
                column: "MangaUserDataId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaChapters_MangaId",
                table: "MangaChapters",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaChaptersHistory_ChapterId",
                table: "MangaChaptersHistory",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaChaptersHistory_UserId",
                table: "MangaChaptersHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaMangaTag_TagsId",
                table: "MangaMangaTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_Mangas_MangaAniListID",
                table: "Mangas",
                column: "MangaAniListID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mangas_MangaMyAnimeListID",
                table: "Mangas",
                column: "MangaMyAnimeListID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mangas_WriterId",
                table: "Mangas",
                column: "WriterId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaSources_MangaId",
                table: "MangaSources",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaStatusHistory_MangaId",
                table: "MangaStatusHistory",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaStatusHistory_UserId",
                table: "MangaStatusHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaSynonym_MangaId",
                table: "MangaSynonym",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaTranslations_MangaChapterId",
                table: "MangaTranslations",
                column: "MangaChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaUsersData_MangaId",
                table: "MangaUsersData",
                column: "MangaId");

            migrationBuilder.CreateIndex(
                name: "IX_MangaUsersData_UserId",
                table: "MangaUsersData",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerMembers_DiscordId",
                table: "ServerMembers",
                column: "DiscordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerMembers_Username",
                table: "ServerMembers",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ServerMemberId",
                table: "Users",
                column: "ServerMemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddedMangaHistory");

            migrationBuilder.DropTable(
                name: "MangaAlternativeNames");

            migrationBuilder.DropTable(
                name: "MangaChapterMangaUserData");

            migrationBuilder.DropTable(
                name: "MangaChaptersHistory");

            migrationBuilder.DropTable(
                name: "MangaMangaTag");

            migrationBuilder.DropTable(
                name: "MangaSources");

            migrationBuilder.DropTable(
                name: "MangaStatusHistory");

            migrationBuilder.DropTable(
                name: "MangaSynonym");

            migrationBuilder.DropTable(
                name: "MangaTranslations");

            migrationBuilder.DropTable(
                name: "MangaUsersData");

            migrationBuilder.DropTable(
                name: "MangaTags");

            migrationBuilder.DropTable(
                name: "MangaChapters");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Mangas");

            migrationBuilder.DropTable(
                name: "ServerMembers");

            migrationBuilder.DropTable(
                name: "MangaWriters");
        }
    }
}
