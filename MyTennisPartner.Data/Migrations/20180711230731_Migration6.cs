using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration6 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Venues_MatchVenueVenueId",
                table: "Matches");

            migrationBuilder.AlterColumn<int>(
                name: "MatchVenueVenueId",
                table: "Matches",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateTable(
                name: "LeagueMemberMatches",
                columns: table => new
                {
                    LeagueMemberId = table.Column<int>(nullable: false),
                    MatchId = table.Column<int>(nullable: false)
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
                name: "IX_LeagueMemberMatches_MatchId",
                table: "LeagueMemberMatches",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Venues_MatchVenueVenueId",
                table: "Matches",
                column: "MatchVenueVenueId",
                principalTable: "Venues",
                principalColumn: "VenueId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Venues_MatchVenueVenueId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "LeagueMemberMatches");

            migrationBuilder.AlterColumn<int>(
                name: "MatchVenueVenueId",
                table: "Matches",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Venues_MatchVenueVenueId",
                table: "Matches",
                column: "MatchVenueVenueId",
                principalTable: "Venues",
                principalColumn: "VenueId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
