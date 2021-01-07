﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using MyTennisPartner.Data.Context;
using MyTennisPartner.Models.Enums;
using System;

namespace MyTennisPartner.Data.Migrations
{
    [DbContext(typeof(TennisContext))]
    [Migration("20180314220734_Migration1")]
    partial class Migration1
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MyTennisPartner.Data.Models.Address", b =>
                {
                    b.Property<int>("AddressId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City")
                        .HasMaxLength(50);

                    b.Property<string>("State")
                        .HasMaxLength(2);

                    b.Property<string>("Street1")
                        .HasMaxLength(50);

                    b.Property<string>("Street2")
                        .HasMaxLength(50);

                    b.Property<int>("VenueId");

                    b.Property<string>("Zip")
                        .HasMaxLength(10);

                    b.HasKey("AddressId");

                    b.HasIndex("VenueId")
                        .IsUnique();

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Contact", b =>
                {
                    b.Property<int>("ContactId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .HasMaxLength(50);

                    b.Property<string>("FirstName")
                        .HasMaxLength(30);

                    b.Property<string>("LastName")
                        .HasMaxLength(30);

                    b.Property<string>("Phone1")
                        .HasMaxLength(30);

                    b.Property<string>("Phone2")
                        .HasMaxLength(30);

                    b.Property<int>("VenueId");

                    b.HasKey("ContactId");

                    b.HasIndex("VenueId")
                        .IsUnique();

                    b.ToTable("Contacts");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.League", b =>
                {
                    b.Property<int>("LeagueId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("DefaultFormat");

                    b.Property<int>("DefaultNumberOfLines")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue(1);

                    b.Property<string>("Description")
                        .HasMaxLength(100);

                    b.Property<string>("Details")
                        .HasMaxLength(1000);

                    b.Property<int?>("HomeVenueVenueId");

                    b.Property<bool>("IsTemplate");

                    b.Property<TimeSpan>("MatchStartTime");

                    b.Property<int>("MaxNumberRegularMembers");

                    b.Property<string>("MaximumRanking");

                    b.Property<int>("MeetingDay");

                    b.Property<int>("MeetingFrequency");

                    b.Property<int>("MinimumAge");

                    b.Property<string>("MinimumRanking");

                    b.Property<string>("Name")
                        .HasMaxLength(52);

                    b.Property<int>("NumberMatchesPerSession");

                    b.Property<int?>("OwnerMemberId");

                    b.Property<bool>("RotatePartners");

                    b.Property<bool>("ScoreTracking");

                    b.Property<int>("WarmupTimeMinutes");

                    b.HasKey("LeagueId");

                    b.HasIndex("HomeVenueVenueId");

                    b.HasIndex("OwnerMemberId");

                    b.ToTable("Leagues");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.LeagueMember", b =>
                {
                    b.Property<int>("LeagueMemberId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsCaptain");

                    b.Property<bool>("IsSubstitute");

                    b.Property<int>("LeagueId");

                    b.Property<int>("MemberId");

                    b.HasKey("LeagueMemberId");

                    b.HasIndex("LeagueId");

                    b.HasIndex("MemberId");

                    b.ToTable("LeagueMembers");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.LeagueMemberLine", b =>
                {
                    b.Property<int>("LeagueMemberLineId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsHomeMember");

                    b.Property<bool>("IsSubstitute");

                    b.Property<int>("LeagueMemberId");

                    b.Property<int>("LineId");

                    b.Property<int>("Rotation");

                    b.Property<int>("Score");

                    b.HasKey("LeagueMemberLineId");

                    b.HasIndex("LeagueMemberId");

                    b.HasIndex("LineId");

                    b.ToTable("LeagueMemberLines");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Line", b =>
                {
                    b.Property<int>("LineId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CourtNumber");

                    b.Property<int>("Format");

                    b.Property<int?>("MatchId");

                    b.HasKey("LineId");

                    b.HasIndex("MatchId");

                    b.ToTable("Lines");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Match", b =>
                {
                    b.Property<int>("MatchId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("EndTime");

                    b.Property<int>("Format");

                    b.Property<bool>("HomeMatch");

                    b.Property<int>("LeagueId");

                    b.Property<int>("MatchVenueVenueId");

                    b.Property<int?>("SessionId");

                    b.Property<DateTime>("StartTime");

                    b.Property<DateTime>("WarmupTime");

                    b.HasKey("MatchId");

                    b.HasIndex("LeagueId");

                    b.HasIndex("MatchVenueVenueId");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Member", b =>
                {
                    b.Property<int>("MemberId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BirthYear");

                    b.Property<string>("FirstName");

                    b.Property<int>("Gender");

                    b.Property<int?>("HomeVenueVenueId");

                    b.Property<string>("LastName");

                    b.Property<string>("SkillRanking");

                    b.Property<string>("UserId");

                    b.Property<string>("ZipCode")
                        .IsRequired()
                        .HasMaxLength(5);

                    b.HasKey("MemberId");

                    b.HasIndex("HomeVenueVenueId");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.MemberImage", b =>
                {
                    b.Property<int>("ImageId")
                        .ValueGeneratedOnAdd();

                    b.Property<byte[]>("ImageBytes");

                    b.Property<int>("MemberId");

                    b.HasKey("ImageId");

                    b.HasIndex("MemberId")
                        .IsUnique();

                    b.ToTable("MemberImages");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.MemberRole", b =>
                {
                    b.Property<int>("MemberRoleId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("MemberId");

                    b.Property<int>("Role");

                    b.HasKey("MemberRoleId");

                    b.HasIndex("MemberId");

                    b.ToTable("MemberRoles");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.PlayerPreference", b =>
                {
                    b.Property<int>("PlayerPreferenceId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Format");

                    b.Property<int>("MemberId");

                    b.HasKey("PlayerPreferenceId");

                    b.HasIndex("MemberId");

                    b.ToTable("PlayerPreferences");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Session", b =>
                {
                    b.Property<int>("SessionId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("EndDate");

                    b.Property<int>("LeagueId");

                    b.Property<string>("Name");

                    b.Property<DateTime>("StartDate");

                    b.HasKey("SessionId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Venue", b =>
                {
                    b.Property<int>("VenueId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("VenueId");

                    b.ToTable("Venues");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Address", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Venue")
                        .WithOne("VenueAddress")
                        .HasForeignKey("MyTennisPartner.Data.Models.Address", "VenueId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Contact", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Venue")
                        .WithOne("VenueContact")
                        .HasForeignKey("MyTennisPartner.Data.Models.Contact", "VenueId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.League", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Venue", "HomeVenue")
                        .WithMany()
                        .HasForeignKey("HomeVenueVenueId");

                    b.HasOne("MyTennisPartner.Data.Models.Member", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerMemberId");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.LeagueMember", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.League", "League")
                        .WithMany("LeagueMembers")
                        .HasForeignKey("LeagueId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MyTennisPartner.Data.Models.Member", "Member")
                        .WithMany("LeagueMembers")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.LeagueMemberLine", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.LeagueMember", "LeagueMember")
                        .WithMany("LeagueMemberLines")
                        .HasForeignKey("LeagueMemberId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MyTennisPartner.Data.Models.Line", "Line")
                        .WithMany("LeagueMemberLines")
                        .HasForeignKey("LineId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Line", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Match")
                        .WithMany("Lines")
                        .HasForeignKey("MatchId");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Match", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.League")
                        .WithMany("Matches")
                        .HasForeignKey("LeagueId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("MyTennisPartner.Data.Models.Venue", "MatchVenue")
                        .WithMany()
                        .HasForeignKey("MatchVenueVenueId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.Member", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Venue", "HomeVenue")
                        .WithMany()
                        .HasForeignKey("HomeVenueVenueId");
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.MemberImage", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Member")
                        .WithOne("Image")
                        .HasForeignKey("MyTennisPartner.Data.Models.MemberImage", "MemberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.MemberRole", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Member")
                        .WithMany("MemberRoles")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("MyTennisPartner.Data.Models.PlayerPreference", b =>
                {
                    b.HasOne("MyTennisPartner.Data.Models.Member")
                        .WithMany("PlayerPreferences")
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
