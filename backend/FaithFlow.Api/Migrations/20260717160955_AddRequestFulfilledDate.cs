using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestFulfilledDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FulfilledDate",
                table: "Requests",
                type: "TEXT",
                nullable: true);

            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(
                    """
                    ALTER TABLE "Requests"
                        ALTER COLUMN "FulfilledDate" TYPE timestamp with time zone
                        USING (
                            CASE
                                WHEN "FulfilledDate" IS NULL OR btrim("FulfilledDate") = '' THEN NULL
                                ELSE "FulfilledDate"::timestamp with time zone
                            END
                        );
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FulfilledDate",
                table: "Requests");
        }
    }
}
