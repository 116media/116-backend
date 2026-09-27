using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Storage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameCoreSchemaToStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Every table InitCoreSchema created moves to the renamed schema, which is then dropped. The
            // migration history is not among them: no context configures a history table, so all four share
            // public.__EFMigrationsHistory and it is untouched by the rename. Idempotent, so it is also a no-op
            // on a database an operator renamed by hand before the deploy.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE moved record;
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'core') THEN
                        CREATE SCHEMA IF NOT EXISTS storage;
                        FOR moved IN SELECT tablename FROM pg_tables WHERE schemaname = 'core' LOOP
                            EXECUTE format('ALTER TABLE core.%I SET SCHEMA storage', moved.tablename);
                        END LOOP;
                        DROP SCHEMA core;
                    END IF;
                END $$;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE moved record;
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'storage') THEN
                        CREATE SCHEMA IF NOT EXISTS core;
                        FOR moved IN SELECT tablename FROM pg_tables WHERE schemaname = 'storage' LOOP
                            EXECUTE format('ALTER TABLE storage.%I SET SCHEMA core', moved.tablename);
                        END LOOP;
                        DROP SCHEMA storage;
                    END IF;
                END $$;
                """
            );
        }
    }
}
