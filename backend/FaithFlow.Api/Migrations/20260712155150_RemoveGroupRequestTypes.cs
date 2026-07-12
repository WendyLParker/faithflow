using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveGroupRequestTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupRequestTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GroupRequestTypes",
                columns: table => new
                {
                    GroupId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRequestTypes", x => new { x.GroupId, x.RequestTypeId });
                    table.ForeignKey(
                        name: "FK_GroupRequestTypes_Groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRequestTypes_RequestTypes_RequestTypeId",
                        column: x => x.RequestTypeId,
                        principalTable: "RequestTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GroupRequestTypes",
                columns: new[] { "GroupId", "RequestTypeId" },
                values: new object[,]
                {
                    { 1, 2 },
                    { 2, 1 },
                    { 3, 3 },
                    { 4, 4 },
                    { 4, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupRequestTypes_RequestTypeId",
                table: "GroupRequestTypes",
                column: "RequestTypeId");
        }
    }
}
