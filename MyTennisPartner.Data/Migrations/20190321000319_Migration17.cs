using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpareTimeMemberNumber",
                table: "Members",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpareTimeMemberNumber",
                table: "Members");
        }
    }
}
