using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoReserveCourts",
                table: "Matches",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReserved",
                table: "Lines",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AutoReserveCourts",
                table: "Leagues",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoReserveCourts",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsReserved",
                table: "Lines");

            migrationBuilder.DropColumn(
                name: "AutoReserveCourts",
                table: "Leagues");
        }
    }
}
