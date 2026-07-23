using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixRequestCommentsPostgresDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // RequestComments.CreatedAt was scaffolded as SQLite TEXT; Npgsql cannot
            // read text columns as DateTime.
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(
                    """
                    DO $$
                    BEGIN
                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = current_schema()
                              AND table_name = 'RequestComments'
                              AND column_name = 'CreatedAt'
                              AND data_type = 'text'
                        ) THEN
                            ALTER TABLE "RequestComments"
                                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                                USING ("CreatedAt"::timestamp with time zone);
                        END IF;
                    END $$;
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.Sql(
                    """
                    DO $$
                    BEGIN
                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = current_schema()
                              AND table_name = 'RequestComments'
                              AND column_name = 'CreatedAt'
                              AND udt_name = 'timestamptz'
                        ) THEN
                            ALTER TABLE "RequestComments"
                                ALTER COLUMN "CreatedAt" TYPE text
                                USING ("CreatedAt"::text);
                        END IF;
                    END $$;
                    """);
            }
        }
    }
}
