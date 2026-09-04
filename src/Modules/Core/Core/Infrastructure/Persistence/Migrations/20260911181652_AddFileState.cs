using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "state",
                schema: "core",
                table: "files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Carry the flag pair over before it is dropped: a deleted row keeps its soft-delete,
            // a claimed row keeps its claim. Deletion and replacement both set is_deleted and are
            // no longer distinguishable, so every soft-deleted row lands on Deleted.
            migrationBuilder.Sql(
                """
                UPDATE core.files
                SET state = CASE
                    WHEN is_deleted THEN 2
                    WHEN claimed_at IS NOT NULL THEN 1
                    ELSE 0
                END;
                """);

            migrationBuilder.DropIndex(
                name: "ix_files_is_deleted",
                schema: "core",
                table: "files");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "core",
                table: "files");

            migrationBuilder.CreateIndex(
                name: "ix_files_state",
                schema: "core",
                table: "files",
                column: "state");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "core",
                table: "files",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                """
                UPDATE core.files
                SET is_deleted = state IN (2, 3);
                """);

            migrationBuilder.DropIndex(
                name: "ix_files_state",
                schema: "core",
                table: "files");

            migrationBuilder.DropColumn(
                name: "state",
                schema: "core",
                table: "files");

            migrationBuilder.CreateIndex(
                name: "ix_files_is_deleted",
                schema: "core",
                table: "files",
                column: "is_deleted");

            // Dropping state took every index filtering on it, so both are rebuilt on is_deleted.
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS core.ix_files_file_name;
                DROP INDEX IF EXISTS core.ix_files_created_at;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files",
                column: "file_name",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_files_created_at",
                schema: "core",
                table: "files",
                column: "created_at",
                filter: "claimed_at IS NULL AND is_deleted = false");
        }
    }
}
