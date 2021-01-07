using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyTennisPartner.Data.Migrations
{
    public partial class Migration16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SpareTimePassword",
                table: "Members",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpareTimeUsername",
                table: "Members",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReservationSystems",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CourtReservationProvider = table.Column<int>(nullable: false),
                    HostName = table.Column<string>(nullable: true),
                    EarliestCourtHour = table.Column<int>(nullable: false),
                    EarliestCourtMinute = table.Column<int>(nullable: false),
                    LatestCourtHour = table.Column<int>(nullable: false),
                    LatestCourtMinute = table.Column<int>(nullable: false),
                    VenueId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationSystems_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSystems_VenueId",
                table: "ReservationSystems",
                column: "VenueId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationSystems");

            migrationBuilder.DropColumn(
                name: "SpareTimePassword",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "SpareTimeUsername",
                table: "Members");
        }
    }
}
