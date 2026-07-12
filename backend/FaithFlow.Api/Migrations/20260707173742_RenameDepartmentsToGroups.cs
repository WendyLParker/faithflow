using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class RenameDepartmentsToGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "Groups");

            migrationBuilder.RenameTable(
                name: "DepartmentRequestTypes",
                newName: "GroupRequestTypes");

            migrationBuilder.RenameTable(
                name: "UserDepartments",
                newName: "UserGroups");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "GroupRequestTypes",
                newName: "GroupId");

            migrationBuilder.RenameColumn(
                name: "DepartmentId",
                table: "UserGroups",
                newName: "GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_UserDepartments_DepartmentId",
                table: "UserGroups",
                newName: "IX_UserGroups_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_UserDepartments_UserId_DepartmentId",
                table: "UserGroups",
                newName: "IX_UserGroups_UserId_GroupId");

            migrationBuilder.RenameIndex(
                name: "IX_DepartmentRequestTypes_RequestTypeId",
                table: "GroupRequestTypes",
                newName: "IX_GroupRequestTypes_RequestTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_GroupRequestTypes_RequestTypeId",
                table: "GroupRequestTypes",
                newName: "IX_DepartmentRequestTypes_RequestTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_UserGroups_UserId_GroupId",
                table: "UserGroups",
                newName: "IX_UserDepartments_UserId_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_UserGroups_GroupId",
                table: "UserGroups",
                newName: "IX_UserDepartments_DepartmentId");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "UserGroups",
                newName: "DepartmentId");

            migrationBuilder.RenameColumn(
                name: "GroupId",
                table: "GroupRequestTypes",
                newName: "DepartmentId");

            migrationBuilder.RenameTable(
                name: "UserGroups",
                newName: "UserDepartments");

            migrationBuilder.RenameTable(
                name: "GroupRequestTypes",
                newName: "DepartmentRequestTypes");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Departments");
        }
    }
}
