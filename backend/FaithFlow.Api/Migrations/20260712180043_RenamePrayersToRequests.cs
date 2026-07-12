using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenamePrayersToRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.RenameTable(
                name: "Prayers",
                newName: "Requests");

            migrationBuilder.RenameColumn(
                name: "PrayerDate",
                table: "Requests",
                newName: "RequestDate");

            migrationBuilder.RenameColumn(
                name: "IsAnswered",
                table: "Requests",
                newName: "IsCompleted");

            migrationBuilder.RenameColumn(
                name: "AnsweredDate",
                table: "Requests",
                newName: "CompletedDate");

            migrationBuilder.DropColumn(
                name: "Categories",
                table: "Requests");

            migrationBuilder.RenameTable(
                name: "PrayerGroups",
                newName: "RequestGroups");

            migrationBuilder.RenameColumn(
                name: "PrayerId",
                table: "RequestGroups",
                newName: "RequestId");

            migrationBuilder.RenameColumn(
                name: "PrayerId",
                table: "Notifications",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_PrayerId",
                table: "Notifications",
                newName: "IX_Notifications_RequestId");

            migrationBuilder.RenameColumn(
                name: "PrayerId",
                table: "ProgressNotes",
                newName: "RequestId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgressNotes_PrayerId",
                table: "ProgressNotes",
                newName: "IX_ProgressNotes_RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_ProgressNotes_RequestId",
                table: "ProgressNotes",
                newName: "IX_ProgressNotes_PrayerId");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "ProgressNotes",
                newName: "PrayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Notifications_RequestId",
                table: "Notifications",
                newName: "IX_Notifications_PrayerId");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "Notifications",
                newName: "PrayerId");

            migrationBuilder.RenameColumn(
                name: "RequestId",
                table: "RequestGroups",
                newName: "PrayerId");

            migrationBuilder.RenameTable(
                name: "RequestGroups",
                newName: "PrayerGroups");

            migrationBuilder.AddColumn<string>(
                name: "Categories",
                table: "Requests",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.RenameColumn(
                name: "CompletedDate",
                table: "Requests",
                newName: "AnsweredDate");

            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "Requests",
                newName: "IsAnswered");

            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "Requests",
                newName: "PrayerDate");

            migrationBuilder.RenameTable(
                name: "Requests",
                newName: "Prayers");

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });
        }
    }
}
