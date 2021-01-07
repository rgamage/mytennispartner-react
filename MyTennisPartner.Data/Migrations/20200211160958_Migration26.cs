using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration26 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MarkNewCourtsReserved",
                table: "Leagues",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MarkNewPlayersConfirmed",
                table: "Leagues",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarkNewCourtsReserved",
                table: "Leagues");

            migrationBuilder.DropColumn(
                name: "MarkNewPlayersConfirmed",
                table: "Leagues");
        }
    }
}
