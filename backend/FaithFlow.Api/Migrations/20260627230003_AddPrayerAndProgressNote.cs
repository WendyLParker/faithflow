using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPrayerAndProgressNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_Prayers_UserId",
                table: "Prayers");

            migrationBuilder.CreateTable(
                name: "ProgressNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    PrayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MoodRating = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressNotes_Prayers_PrayerId",
                        column: x => x.PrayerId,
                        principalTable: "Prayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProgressNotes_PrayerId",
                table: "ProgressNotes",
                column: "PrayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressNotes");

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MoodRating = table.Column<int>(type: "INTEGER", nullable: true),
                    PrayerId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prayers_UserId",
                table: "Prayers",
                column: "UserId");
        }
    }
}
