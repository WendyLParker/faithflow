using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixUserGroupsCanManageBoolean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CanManage was added with SQLite INTEGER type; Postgres requires boolean for predicates.
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
                              AND table_name = 'UserGroups'
                              AND column_name = 'CanManage'
                              AND data_type <> 'boolean'
                        ) THEN
                            ALTER TABLE "UserGroups"
                                ALTER COLUMN "CanManage" TYPE boolean
                                USING ("CanManage" <> 0);
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
                              AND table_name = 'UserGroups'
                              AND column_name = 'CanManage'
                              AND data_type = 'boolean'
                        ) THEN
                            ALTER TABLE "UserGroups"
                                ALTER COLUMN "CanManage" TYPE integer
                                USING (CASE WHEN "CanManage" THEN 1 ELSE 0 END);
                        END IF;
                    END $$;
                    """);
            }
        }
    }
}
