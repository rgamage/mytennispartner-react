using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeagueMemberLines");

            migrationBuilder.DropTable(
                name: "LeagueMemberMatches");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                    PlayerAvailability = table.Column<int>(nullable: false),
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

            migrationBuilder.CreateTable(
                name: "LeagueMemberMatches",
                columns: table => new
                {
                    LeagueMemberId = table.Column<int>(nullable: false),
                    MatchId = table.Column<int>(nullable: false),
                    Availability = table.Column<int>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueMemberMatches", x => new { x.LeagueMemberId, x.MatchId });
                    table.ForeignKey(
                        name: "FK_LeagueMemberMatches_LeagueMembers_LeagueMemberId",
                        column: x => x.LeagueMemberId,
                        principalTable: "LeagueMembers",
                        principalColumn: "LeagueMemberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueMemberMatches_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMemberLines_LeagueMemberId",
                table: "LeagueMemberLines",
                column: "LeagueMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMemberLines_LineId",
                table: "LeagueMemberLines",
                column: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMemberMatches_MatchId",
                table: "LeagueMemberMatches",
                column: "MatchId");
        }
    }
}
