using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Players_Lines_LineId",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_LineId",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "LineId",
                table: "Players",
                newName: "LineIdNumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LineIdNumber",
                table: "Players",
                newName: "LineId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_LineId",
                table: "Players",
                column: "LineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Players_Lines_LineId",
                table: "Players",
                column: "LineId",
                principalTable: "Lines",
                principalColumn: "LineId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
