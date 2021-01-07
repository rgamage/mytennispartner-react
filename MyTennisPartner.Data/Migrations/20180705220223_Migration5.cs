using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotifyAddOrRemoveMeFromMatch",
                table: "Members",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyCourtChange",
                table: "Members",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifyMatchDetailsChangeOrCancelled",
                table: "Members",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotifySubForMatchOpening",
                table: "Members",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyAddOrRemoveMeFromMatch",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "NotifyCourtChange",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "NotifyMatchDetailsChangeOrCancelled",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "NotifySubForMatchOpening",
                table: "Members");
        }
    }
}
