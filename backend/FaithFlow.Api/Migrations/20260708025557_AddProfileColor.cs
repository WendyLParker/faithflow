using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfileColor",
                table: "UserRoles",
                type: "TEXT",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileColor",
                table: "UserRoles");
        }
    }
}
