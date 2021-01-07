using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Matches_MatchId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "IsUnableToPlay",
                table: "LeagueMemberLines");

            migrationBuilder.AlterColumn<int>(
                name: "MatchId",
                table: "Lines",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayerAvailability",
                table: "LeagueMemberLines",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Matches_MatchId",
                table: "Lines",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lines_Matches_MatchId",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "PlayerAvailability",
                table: "LeagueMemberLines");

            migrationBuilder.AlterColumn<int>(
                name: "MatchId",
                table: "Lines",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<bool>(
                name: "IsUnableToPlay",
                table: "LeagueMemberLines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Lines_Matches_MatchId",
                table: "Lines",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "MatchId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
