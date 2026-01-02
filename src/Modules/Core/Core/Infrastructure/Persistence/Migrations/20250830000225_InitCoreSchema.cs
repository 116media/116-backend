using System;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitCoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.CreateTable(
                name: "files",
                schema: "core",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    storage_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_files", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files",
                column: "file_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_files_is_deleted",
                schema: "core",
                table: "files",
                column: "is_deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "files",
                schema: "core");
        }
    }
}
