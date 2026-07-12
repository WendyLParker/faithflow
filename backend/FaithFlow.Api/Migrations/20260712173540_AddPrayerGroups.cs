using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPrayerGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrayerGroups",
                columns: table => new
                {
                    PrayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrayerGroups", x => new { x.PrayerId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_PrayerGroups_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PrayerGroups_Prayers_PrayerId",
                        column: x => x.PrayerId,
                        principalTable: "Prayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrayerGroups_GroupId",
                table: "PrayerGroups",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrayerGroups");
        }
    }
}
