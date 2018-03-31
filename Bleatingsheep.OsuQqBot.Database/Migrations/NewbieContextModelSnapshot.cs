﻿// <auto-generated />
using Bleatingsheep.OsuMixedApi;
using Bleatingsheep.OsuQqBot.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    [DbContext(typeof(NewbieContext))]
    partial class NewbieContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.Chart", b =>
                {
                    b.Property<int>("ChartId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ChartCreator");

                    b.Property<string>("ChartDescription")
                        .IsRequired();

                    b.Property<string>("ChartName")
                        .IsRequired();

                    b.Property<DateTimeOffset?>("EndTime");

                    b.Property<bool>("IsRunning");

                    b.Property<double?>("MaximumPerformance");

                    b.Property<bool>("Public");

                    b.Property<double>("RecommendPerformance");

                    b.Property<DateTimeOffset>("StartTime");

                    b.HasKey("ChartId");

                    b.ToTable("Charts");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartAdministrator", b =>
                {
                    b.Property<int>("ChartId");

                    b.Property<long>("Administrator");

                    b.HasKey("ChartId", "Administrator");

                    b.ToTable("ChartAdministrators");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartBeatmap", b =>
                {
                    b.Property<int>("ChartId");

                    b.Property<int>("BeatmapId");

                    b.Property<int>("Mode");

                    b.Property<bool>("AllowsFail");

                    b.Property<int>("BannedMods");

                    b.Property<int>("ForceMods");

                    b.Property<int>("RequiredMods");

                    b.Property<string>("ScoreCalculation");

                    b.HasKey("ChartId", "BeatmapId", "Mode");

                    b.ToTable("ChartBeatmaps");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartCommit", b =>
                {
                    b.Property<int>("ChartId");

                    b.Property<int>("BeatmapId");

                    b.Property<int>("Mode");

                    b.Property<long>("Date");

                    b.Property<double>("Accuracy");

                    b.Property<int>("Combo");

                    b.Property<int>("Mods");

                    b.Property<double>("PPWhenCommit");

                    b.Property<string>("Rank")
                        .IsRequired();

                    b.Property<long>("Score");

                    b.Property<int>("Uid");

                    b.HasKey("ChartId", "BeatmapId", "Mode", "Date");

                    b.ToTable("ChartCommits");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartValidGroup", b =>
                {
                    b.Property<int>("ChartId");

                    b.Property<long>("GroupId");

                    b.HasKey("ChartId", "GroupId");

                    b.ToTable("ChartValidGroups");
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartAdministrator", b =>
                {
                    b.HasOne("Bleatingsheep.OsuQqBot.Database.Models.Chart")
                        .WithMany("ChartAdministrators")
                        .HasForeignKey("ChartId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartBeatmap", b =>
                {
                    b.HasOne("Bleatingsheep.OsuQqBot.Database.Models.Chart")
                        .WithMany("Maps")
                        .HasForeignKey("ChartId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartCommit", b =>
                {
                    b.HasOne("Bleatingsheep.OsuQqBot.Database.Models.ChartBeatmap")
                        .WithMany("Commits")
                        .HasForeignKey("ChartId", "BeatmapId", "Mode")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Bleatingsheep.OsuQqBot.Database.Models.ChartValidGroup", b =>
                {
                    b.HasOne("Bleatingsheep.OsuQqBot.Database.Models.Chart")
                        .WithMany("Groups")
                        .HasForeignKey("ChartId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
