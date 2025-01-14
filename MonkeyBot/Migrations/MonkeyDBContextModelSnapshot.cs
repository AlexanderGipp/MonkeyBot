﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MonkeyBot.Database;

namespace MonkeyBot.Migrations
{
    [DbContext(typeof(MonkeyDBContext))]
    partial class MonkeyDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.9");

            modelBuilder.Entity("MonkeyBot.Models.Announcement", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CronExpression")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ExecutionTime")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Announcements");
                });

            modelBuilder.Entity("MonkeyBot.Models.BenzenFact", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Fact")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("BenzenFacts");
                });

            modelBuilder.Entity("MonkeyBot.Models.Feed", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastUpdate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("URL")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Feeds");
                });

            modelBuilder.Entity("MonkeyBot.Models.GameServer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GameServerType")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("GameVersion")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastVersionUpdate")
                        .HasColumnType("TEXT");

                    b.Property<ulong?>("MessageID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ServerIP")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("GameServers");
                });

            modelBuilder.Entity("MonkeyBot.Models.GuildConfig", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("BattlefieldUpdatesChannel")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("BattlefieldUpdatesEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<string>("CommandPrefix")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConfirmedStreamerIds")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("DefaultChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GiveAwayChannel")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("GiveAwaysEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<ulong>("GoodbyeMessageChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("GoodbyeMessageText")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("LastBattlefieldUpdate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastGiveAway")
                        .HasColumnType("TEXT");

                    b.Property<string>("Rules")
                        .HasColumnType("TEXT");

                    b.Property<bool>("StreamAnnouncementsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(false);

                    b.Property<ulong>("WelcomeMessageChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WelcomeMessageText")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("GuildConfigs");
                });

            modelBuilder.Entity("MonkeyBot.Models.Poll", b =>
                {
                    b.Property<int?>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("CreatorId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndTimeUTC")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MessageId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PossibleAnswers")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Question")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Polls");
                });

            modelBuilder.Entity("MonkeyBot.Models.RoleButtonLink", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("EmoteString")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("MessageID")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("RoleID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("RoleButtonLinks");
                });

            modelBuilder.Entity("MonkeyBot.Models.TriviaScore", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GuildID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("TriviaScores");
                });
#pragma warning restore 612, 618
        }
    }
}
