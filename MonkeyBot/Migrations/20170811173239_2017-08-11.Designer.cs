﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using MonkeyBot.Database;
using MonkeyBot.Database.Entities;

namespace MonkeyBot.Migrations
{
    [DbContext(typeof(MonkeyDBContext))]
    [Migration("20170811173239_2017-08-11")]
    partial class _20170811
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("MonkeyBot.Database.Entities.AnnouncementEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("ChannelId");

                    b.Property<string>("CronExpression");

                    b.Property<DateTime?>("ExecutionTime");

                    b.Property<ulong>("GuildId");

                    b.Property<string>("Message")
                        .IsRequired();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("Announcements");
                });

            modelBuilder.Entity("MonkeyBot.Database.Entities.BenzenFactEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Fact");

                    b.HasKey("Id");

                    b.ToTable("BenzenFacts");
                });

            modelBuilder.Entity("MonkeyBot.Database.Entities.GuildConfigEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CommandPrefix")
                        .IsRequired();

                    b.Property<string>("FeedUrlsAsString")
                        .HasColumnName("FeedUrls");

                    b.Property<ulong>("GuildId");

                    b.Property<bool>("ListenToFeeds");

                    b.Property<string>("RulesAsString")
                        .HasColumnName("Rules");

                    b.Property<string>("WelcomeMessageText");

                    b.HasKey("Id");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("MonkeyBot.Database.Entities.TriviaScoreEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<ulong>("GuildID");

                    b.Property<int>("Score");

                    b.Property<ulong>("UserID");

                    b.HasKey("Id");

                    b.ToTable("TriviaScores");
                });
        }
    }
}