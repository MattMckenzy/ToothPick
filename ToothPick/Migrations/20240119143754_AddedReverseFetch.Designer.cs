﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ToothPick.Models;

#nullable disable

namespace ToothPick.Migrations
{
    [DbContext(typeof(ToothPickContext))]
    [Migration("20240119143754_AddedReverseFetch")]
    partial class AddedReverseFetch
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0-rc.1.23419.6")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("ToothPick.Models.Library", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("Libraries");
                });

            modelBuilder.Entity("ToothPick.Models.Location", b =>
                {
                    b.Property<string>("LibraryName")
                        .HasColumnType("TEXT");

                    b.Property<string>("SeriesName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Cookies")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DownloadFormat")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("FetchCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MatchFilters")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("ReverseFetch")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LibraryName", "SeriesName", "Name");

                    b.ToTable("Locations");
                });

            modelBuilder.Entity("ToothPick.Models.Media", b =>
                {
                    b.Property<string>("LibraryName")
                        .HasColumnType("TEXT");

                    b.Property<string>("SeriesName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Url")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("DatePublished")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<float?>("Duration")
                        .HasColumnType("REAL");

                    b.Property<int?>("EpisodeNumber")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SeasonNumber")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ThumbnailLocation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LibraryName", "SeriesName", "Url");

                    b.ToTable("Media");
                });

            modelBuilder.Entity("ToothPick.Models.Series", b =>
                {
                    b.Property<string>("LibraryName")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("BannerLocation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("LogoLocation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("PosterLocation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbnailLocation")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LibraryName", "Name");

                    b.ToTable("Series");
                });

            modelBuilder.Entity("ToothPick.Models.Setting", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Name");

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("ToothPick.Models.Location", b =>
                {
                    b.HasOne("ToothPick.Models.Library", "Library")
                        .WithMany("Locations")
                        .HasForeignKey("LibraryName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ToothPick.Models.Series", "Series")
                        .WithMany("Locations")
                        .HasForeignKey("LibraryName", "SeriesName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Library");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("ToothPick.Models.Media", b =>
                {
                    b.HasOne("ToothPick.Models.Library", "Library")
                        .WithMany("Medias")
                        .HasForeignKey("LibraryName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ToothPick.Models.Series", "Series")
                        .WithMany("Medias")
                        .HasForeignKey("LibraryName", "SeriesName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Library");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("ToothPick.Models.Series", b =>
                {
                    b.HasOne("ToothPick.Models.Library", "Library")
                        .WithMany("Series")
                        .HasForeignKey("LibraryName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Library");
                });

            modelBuilder.Entity("ToothPick.Models.Library", b =>
                {
                    b.Navigation("Locations");

                    b.Navigation("Medias");

                    b.Navigation("Series");
                });

            modelBuilder.Entity("ToothPick.Models.Series", b =>
                {
                    b.Navigation("Locations");

                    b.Navigation("Medias");
                });
#pragma warning restore 612, 618
        }
    }
}
