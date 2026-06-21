using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeFileNameUniqueIndexPartial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files");

            migrationBuilder.CreateIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files",
                column: "file_name",
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files");

            migrationBuilder.CreateIndex(
                name: "ix_files_file_name",
                schema: "core",
                table: "files",
                column: "file_name",
                unique: true);
        }
    }
}
