using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FaithFlow.Backend.Migrations
{
    /// <inheritdoc />
    public partial class FixPostgresDateTimeColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Several migrations were scaffolded for SQLite and created DateTime columns
            // as TEXT on PostgreSQL. Npgsql cannot read text as DateTime.
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
                return;

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'Requests'
                          AND column_name = 'RequestDate'
                          AND data_type = 'text'
                    ) THEN
                        ALTER TABLE "Requests"
                            ALTER COLUMN "RequestDate" TYPE timestamp with time zone
                            USING ("RequestDate"::timestamp with time zone);
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'Requests'
                          AND column_name = 'CompletedDate'
                          AND data_type = 'text'
                    ) THEN
                        ALTER TABLE "Requests"
                            ALTER COLUMN "CompletedDate" TYPE timestamp with time zone
                            USING (
                                CASE
                                    WHEN "CompletedDate" IS NULL OR btrim("CompletedDate") = '' THEN NULL
                                    ELSE "CompletedDate"::timestamp with time zone
                                END
                            );
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'Requests'
                          AND column_name = 'FulfilledDate'
                          AND data_type = 'text'
                    ) THEN
                        ALTER TABLE "Requests"
                            ALTER COLUMN "FulfilledDate" TYPE timestamp with time zone
                            USING (
                                CASE
                                    WHEN "FulfilledDate" IS NULL OR btrim("FulfilledDate") = '' THEN NULL
                                    ELSE "FulfilledDate"::timestamp with time zone
                                END
                            );
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'ProgressNotes'
                          AND column_name = 'EntryDate'
                          AND data_type = 'text'
                    ) THEN
                        ALTER TABLE "ProgressNotes"
                            ALTER COLUMN "EntryDate" TYPE timestamp with time zone
                            USING ("EntryDate"::timestamp with time zone);
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_schema = current_schema()
                          AND table_name = 'Notifications'
                          AND column_name = 'CreatedAt'
                          AND data_type = 'text'
                    ) THEN
                        ALTER TABLE "Notifications"
                            ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                            USING ("CreatedAt"::timestamp with time zone);
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No down migration — reverting to text would break Npgsql reads again.
        }
    }
}
