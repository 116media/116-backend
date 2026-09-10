using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileClaimedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "claimed_at",
                schema: "core",
                table: "files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_files_created_at",
                schema: "core",
                table: "files",
                column: "created_at",
                filter: "claimed_at IS NULL AND is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_files_created_at",
                schema: "core",
                table: "files");

            migrationBuilder.DropColumn(
                name: "claimed_at",
                schema: "core",
                table: "files");
        }
    }
}
