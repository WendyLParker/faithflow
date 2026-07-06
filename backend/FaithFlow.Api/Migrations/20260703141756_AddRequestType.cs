using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "RequestTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Ride" },
                    { 2, "Prayer" },
                    { 3, "Supply" },
                    { 4, "Service" },
                    { 5, "Labor" }
                });

            migrationBuilder.AddColumn<int>(
                name: "RequestTypeId",
                table: "Prayers",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Prayers_RequestTypeId",
                table: "Prayers",
                column: "RequestTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prayers_RequestTypes_RequestTypeId",
                table: "Prayers",
                column: "RequestTypeId",
                principalTable: "RequestTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prayers_RequestTypes_RequestTypeId",
                table: "Prayers");

            migrationBuilder.DropTable(
                name: "RequestTypes");

            migrationBuilder.DropIndex(
                name: "IX_Prayers_RequestTypeId",
                table: "Prayers");

            migrationBuilder.DropColumn(
                name: "RequestTypeId",
                table: "Prayers");
        }
    }
}
