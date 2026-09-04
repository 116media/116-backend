using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CollapseFileStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE core.files
                SET state = CASE state
                    WHEN 1 THEN 0
                    WHEN 2 THEN 1
                    WHEN 3 THEN 2
                    ELSE 0
                END;
                """);

            // Dropping is_deleted took both partial indexes with it, so neither is dropped here.
            migrationBuilder.DropColumn(
                name: "claimed_at",
                schema: "core",
                table: "files");

            migrationBuilder.CreateIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files",
                column: "file_name",
                unique: true,
                filter: "state = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files");

            migrationBuilder.AddColumn<DateTime>(
                name: "claimed_at",
                schema: "core",
                table: "files",
                type: "timestamp with time zone",
                nullable: true);

            // Stored rows all land on Unclaimed: the claim stamp they carried is gone for good.
            migrationBuilder.Sql(
                """
                UPDATE core.files
                SET state = CASE state
                    WHEN 1 THEN 2
                    WHEN 2 THEN 3
                    ELSE 0
                END;
                """);

            migrationBuilder.CreateIndex(
                name: "ix_files_created_at",
                schema: "core",
                table: "files",
                column: "created_at",
                filter: "claimed_at IS NULL AND state NOT IN (2, 3)");

            migrationBuilder.CreateIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files",
                column: "file_name",
                unique: true,
                filter: "state NOT IN (2, 3)");
        }
    }
}
