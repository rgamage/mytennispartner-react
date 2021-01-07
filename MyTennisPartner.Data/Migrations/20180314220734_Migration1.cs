using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Venues_MatchVenueVenueId",
                table: "Matches");

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Venues_MatchVenueVenueId",
                table: "Matches");

            migrationBuilder.AlterColumn<int>(
                name: "MatchVenueVenueId",
                table: "Matches",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Venues_MatchVenueVenueId",
                table: "Matches",
                column: "MatchVenueVenueId",
                principalTable: "Venues",
                principalColumn: "VenueId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
