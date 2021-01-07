using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration25 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE - removed these migrations because they were applied manually.  Sorry.
            //migrationBuilder.AddColumn<string>(
            //    name: "Email",
            //    table: "Members",
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "EmailNotificationFlags",
            //    table: "Members",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<int>(
            //    name: "TextNotificationFlags",
            //    table: "Members",
            //    nullable: false,
            //    defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "Email",
            //    table: "Members");

            //migrationBuilder.DropColumn(
            //    name: "EmailNotificationFlags",
            //    table: "Members");

            //migrationBuilder.DropColumn(
            //    name: "TextNotificationFlags",
            //    table: "Members");
        }
    }
}
