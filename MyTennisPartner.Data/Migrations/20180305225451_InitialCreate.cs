using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MyTennisPartner.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    SessionId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EndDate = table.Column<DateTime>(nullable: false),
                    LeagueId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StartDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.SessionId);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    VenueId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueId);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    AddressId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    City = table.Column<string>(maxLength: 50, nullable: true),
                    State = table.Column<string>(maxLength: 2, nullable: true),
                    Street1 = table.Column<string>(maxLength: 50, nullable: true),
                    Street2 = table.Column<string>(maxLength: 50, nullable: true),
                    VenueId = table.Column<int>(nullable: false),
                    Zip = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_Addresses_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    ContactId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(maxLength: 30, nullable: true),
                    LastName = table.Column<string>(maxLength: 30, nullable: true),
                    Phone1 = table.Column<string>(maxLength: 30, nullable: true),
                    Phone2 = table.Column<string>(maxLength: 30, nullable: true),
                    VenueId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.ContactId);
                    table.ForeignKey(
                        name: "FK_Contacts_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    MemberId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BirthYear = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(nullable: true),
                    Gender = table.Column<int>(nullable: false),
                    HomeVenueVenueId = table.Column<int>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    SkillRanking = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    ZipCode = table.Column<string>(maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.MemberId);
                    table.ForeignKey(
                        name: "FK_Members_Venues_HomeVenueVenueId",
                        column: x => x.HomeVenueVenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    LeagueId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DefaultFormat = table.Column<int>(nullable: false),
                    DefaultNumberOfLines = table.Column<int>(nullable: false, defaultValue: 1),
                    Description = table.Column<string>(maxLength: 100, nullable: true),
                    Details = table.Column<string>(maxLength: 1000, nullable: true),
                    HomeVenueVenueId = table.Column<int>(nullable: true),
                    IsTemplate = table.Column<bool>(nullable: false),
                    MatchStartTime = table.Column<TimeSpan>(nullable: false),
                    MaxNumberRegularMembers = table.Column<int>(nullable: false),
                    MaximumRanking = table.Column<string>(nullable: true),
                    MeetingDay = table.Column<int>(nullable: false),
                    MeetingFrequency = table.Column<int>(nullable: false),
                    MinimumAge = table.Column<int>(nullable: false),
                    MinimumRanking = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 52, nullable: true),
                    NumberMatchesPerSession = table.Column<int>(nullable: false),
                    OwnerMemberId = table.Column<int>(nullable: true),
                    RotatePartners = table.Column<bool>(nullable: false),
                    ScoreTracking = table.Column<bool>(nullable: false),
                    WarmupTimeMinutes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.LeagueId);
                    table.ForeignKey(
                        name: "FK_Leagues_Venues_HomeVenueVenueId",
                        column: x => x.HomeVenueVenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leagues_Members_OwnerMemberId",
                        column: x => x.OwnerMemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberImages",
                columns: table => new
                {
                    ImageId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ImageBytes = table.Column<byte[]>(nullable: true),
                    MemberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberImages", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_MemberImages_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberRoles",
                columns: table => new
                {
                    MemberRoleId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MemberId = table.Column<int>(nullable: false),
                    Role = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRoles", x => x.MemberRoleId);
                    table.ForeignKey(
                        name: "FK_MemberRoles_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerPreferences",
                columns: table => new
                {
                    PlayerPreferenceId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Format = table.Column<int>(nullable: false),
                    MemberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerPreferences", x => x.PlayerPreferenceId);
                    table.ForeignKey(
                        name: "FK_PlayerPreferences_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeagueMembers",
                columns: table => new
                {
                    LeagueMemberId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsCaptain = table.Column<bool>(nullable: false),
                    IsSubstitute = table.Column<bool>(nullable: false),
                    LeagueId = table.Column<int>(nullable: false),
                    MemberId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueMembers", x => x.LeagueMemberId);
                    table.ForeignKey(
                        name: "FK_LeagueMembers_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueMembers_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    MatchId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EndTime = table.Column<DateTime>(nullable: false),
                    Format = table.Column<int>(nullable: false),
                    HomeMatch = table.Column<bool>(nullable: false),
                    LeagueId = table.Column<int>(nullable: false),
                    MatchVenueVenueId = table.Column<int>(nullable: true),
                    SessionId = table.Column<int>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    WarmupTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.MatchId);
                    table.ForeignKey(
                        name: "FK_Matches_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "LeagueId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Matches_Venues_MatchVenueVenueId",
                        column: x => x.MatchVenueVenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Lines",
                columns: table => new
                {
                    LineId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourtNumber = table.Column<int>(nullable: false),
                    Format = table.Column<int>(nullable: false),
                    MatchId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lines", x => x.LineId);
                    table.ForeignKey(
                        name: "FK_Lines_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeagueMemberLines",
                columns: table => new
                {
                    LeagueMemberLineId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    IsHomeMember = table.Column<bool>(nullable: false),
                    IsSubstitute = table.Column<bool>(nullable: false),
                    LeagueMemberId = table.Column<int>(nullable: false),
                    LineId = table.Column<int>(nullable: false),
                    Rotation = table.Column<int>(nullable: false),
                    Score = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueMemberLines", x => x.LeagueMemberLineId);
                    table.ForeignKey(
                        name: "FK_LeagueMemberLines_LeagueMembers_LeagueMemberId",
                        column: x => x.LeagueMemberId,
                        principalTable: "LeagueMembers",
                        principalColumn: "LeagueMemberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueMemberLines_Lines_LineId",
                        column: x => x.LineId,
                        principalTable: "Lines",
                        principalColumn: "LineId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_VenueId",
                table: "Addresses",
                column: "VenueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contacts_VenueId",
                table: "Contacts",
                column: "VenueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMemberLines_LeagueMemberId",
                table: "LeagueMemberLines",
                column: "LeagueMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMemberLines_LineId",
                table: "LeagueMemberLines",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_LeagueId",
                table: "LeagueMembers",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_MemberId",
                table: "LeagueMembers",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_HomeVenueVenueId",
                table: "Leagues",
                column: "HomeVenueVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_OwnerMemberId",
                table: "Leagues",
                column: "OwnerMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Lines_MatchId",
                table: "Lines",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_LeagueId",
                table: "Matches",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_MatchVenueVenueId",
                table: "Matches",
                column: "MatchVenueVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberImages_MemberId",
                table: "MemberImages",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberRoles_MemberId",
                table: "MemberRoles",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_HomeVenueVenueId",
                table: "Members",
                column: "HomeVenueVenueId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerPreferences_MemberId",
                table: "PlayerPreferences",
                column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "LeagueMemberLines");

            migrationBuilder.DropTable(
                name: "MemberImages");

            migrationBuilder.DropTable(
                name: "MemberRoles");

            migrationBuilder.DropTable(
                name: "PlayerPreferences");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "LeagueMembers");

            migrationBuilder.DropTable(
                name: "Lines");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Venues");
        }
    }
}
