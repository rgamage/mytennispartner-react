using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration22 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CourtNumberOverridden",
                table: "Lines",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourtNumberOverridden",
                table: "Lines");
        }
    }
}
