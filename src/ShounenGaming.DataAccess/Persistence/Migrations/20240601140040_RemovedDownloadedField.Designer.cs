﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using ShounenGaming.DataAccess.Persistence;

#nullable disable

namespace ShounenGaming.DataAccess.Persistence.Migrations
{
    [DbContext(typeof(ShounenGamingContext))]
    [Migration("20240601140040_RemovedDownloadedField")]
    partial class RemovedDownloadedField
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.3")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true)
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MangaChapterMangaUserData", b =>
                {
                    b.Property<int>("ChaptersReadId")
                        .HasColumnType("integer");

                    b.Property<int>("MangaUserDataId")
                        .HasColumnType("integer");

                    b.HasKey("ChaptersReadId", "MangaUserDataId");

                    b.HasIndex("MangaUserDataId");

                    b.ToTable("MangaChapterMangaUserData");
                });

            modelBuilder.Entity("MangaMangaTag", b =>
                {
                    b.Property<int>("MangasId")
                        .HasColumnType("integer");

                    b.Property<int>("TagsId")
                        .HasColumnType("integer");

                    b.HasKey("MangasId", "TagsId");

                    b.HasIndex("TagsId");

                    b.ToTable("MangaMangaTag");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Base.ServerMember", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DiscordId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DiscordId")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("ServerMembers");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Base.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("Birthday")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("DiscordVerified")
                        .HasColumnType("boolean");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("RefreshTokenExpiryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int?>("ServerMemberId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ServerMemberId")
                        .IsUnique();

                    b.HasIndex("Username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Base.UserMangasConfigurations", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("NSFWBehaviour")
                        .HasColumnType("integer");

                    b.Property<int>("ReadingMode")
                        .HasColumnType("integer");

                    b.Property<bool>("ShowProgressForChaptersWithDecimals")
                        .HasColumnType("boolean");

                    b.Property<bool>("SkipChapterToAnotherTranslation")
                        .HasColumnType("boolean");

                    b.Property<int>("TranslationLanguage")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserMangasConfigurations");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.AddedMangaAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MangaId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MangaId");

                    b.HasIndex("UserId");

                    b.ToTable("AddedMangaHistory");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.ChangedChapterStateAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("ChapterId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Read")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ChapterId");

                    b.HasIndex("UserId");

                    b.ToTable("MangaChaptersHistory");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.ChangedMangaStatusAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MangaId")
                        .HasColumnType("integer");

                    b.Property<int?>("NewState")
                        .HasColumnType("integer");

                    b.Property<int?>("PreviousState")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MangaId");

                    b.HasIndex("UserId");

                    b.ToTable("MangaStatusHistory");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.Manga", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int?>("ALPopularity")
                        .HasColumnType("integer");

                    b.Property<double?>("ALScore")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("FinishedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<List<string>>("ImagesUrls")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<bool>("IsNSFW")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsReleasing")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsSeasonManga")
                        .HasColumnType("boolean");

                    b.Property<int?>("MALPopularity")
                        .HasColumnType("integer");

                    b.Property<double?>("MALScore")
                        .HasColumnType("double precision");

                    b.Property<long?>("MangaAniListID")
                        .HasColumnType("bigint");

                    b.Property<long?>("MangaMyAnimeListID")
                        .HasColumnType("bigint");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("StartedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("WriterId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("MangaAniListID")
                        .IsUnique();

                    b.HasIndex("MangaMyAnimeListID")
                        .IsUnique();

                    b.HasIndex("WriterId");

                    b.ToTable("Mangas");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaAlternativeName", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("MangaId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MangaId");

                    b.ToTable("MangaAlternativeNames");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaChapter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MangaId")
                        .HasColumnType("integer");

                    b.Property<double>("Name")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("MangaId");

                    b.ToTable("MangaChapters");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("ImageURL")
                        .HasColumnType("text");

                    b.Property<int>("MangaId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Source")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MangaId");

                    b.ToTable("MangaSources");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaSynonym", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("MangaId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("MangaId");

                    b.ToTable("MangaSynonym");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("MangaTags");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaTranslation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsWorking")
                        .HasColumnType("boolean");

                    b.Property<int>("Language")
                        .HasColumnType("integer");

                    b.Property<int>("MangaChapterId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReleasedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("MangaChapterId");

                    b.ToTable("MangaTranslations");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaUserData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MangaId")
                        .HasColumnType("integer");

                    b.Property<double?>("Rating")
                        .HasColumnType("double precision");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasAlternateKey("UserId", "MangaId");

                    b.HasIndex("MangaId");

                    b.ToTable("MangaUsersData");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaWriter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("MangaWriters");
                });

            modelBuilder.Entity("MangaChapterMangaUserData", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.MangaChapter", null)
                        .WithMany()
                        .HasForeignKey("ChaptersReadId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShounenGaming.Core.Entities.Mangas.MangaUserData", null)
                        .WithMany()
                        .HasForeignKey("MangaUserDataId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MangaMangaTag", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", null)
                        .WithMany()
                        .HasForeignKey("MangasId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShounenGaming.Core.Entities.Mangas.MangaTag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Base.User", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Base.ServerMember", "ServerMember")
                        .WithOne("User")
                        .HasForeignKey("ShounenGaming.Core.Entities.Base.User", "ServerMemberId");

                    b.Navigation("ServerMember");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Base.UserMangasConfigurations", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Base.User", "User")
                        .WithOne("MangasConfigurations")
                        .HasForeignKey("ShounenGaming.Core.Entities.Base.UserMangasConfigurations", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.AddedMangaAction", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", "Manga")
                        .WithMany()
                        .HasForeignKey("MangaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShounenGaming.Core.Entities.Base.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Manga");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.ChangedChapterStateAction", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.MangaChapter", "Chapter")
                        .WithMany()
                        .HasForeignKey("ChapterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShounenGaming.Core.Entities.Base.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Chapter");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.ChangedMangaStatusAction", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", "Manga")
                        .WithMany()
                        .HasForeignKey("MangaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShounenGaming.Core.Entities.Base.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Manga");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.Manga", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.MangaWriter", "Writer")
                        .WithMany("Mangas")
                        .HasForeignKey("WriterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Writer");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaAlternativeName", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", null)
                        .WithMany("AlternativeNames")
                        .HasForeignKey("MangaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaChapter", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", "Manga")
                        .WithMany("Chapters")
                        .HasForeignKey("MangaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Manga");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaSource", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", null)
                        .WithMany("Sources")
                        .HasForeignKey("MangaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaSynonym", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", "Manga")
                        .WithMany("Synonyms")
                        .HasForeignKey("MangaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Manga");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaTranslation", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.MangaChapter", "MangaChapter")
                        .WithMany("Translations")
                        .HasForeignKey("MangaChapterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MangaChapter");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaUserData", b =>
                {
                    b.HasOne("ShounenGaming.Core.Entities.Mangas.Manga", "Manga")
                        .WithMany("UsersData")
                        .HasForeignKey("MangaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ShounenGaming.Core.Entities.Base.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Manga");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Base.ServerMember", b =>
                {
                    b.Navigation("User");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Base.User", b =>
                {
                    b.Navigation("MangasConfigurations")
                        .IsRequired();
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.Manga", b =>
                {
                    b.Navigation("AlternativeNames");

                    b.Navigation("Chapters");

                    b.Navigation("Sources");

                    b.Navigation("Synonyms");

                    b.Navigation("UsersData");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaChapter", b =>
                {
                    b.Navigation("Translations");
                });

            modelBuilder.Entity("ShounenGaming.Core.Entities.Mangas.MangaWriter", b =>
                {
                    b.Navigation("Mangas");
                });
#pragma warning restore 612, 618
        }
    }
}
