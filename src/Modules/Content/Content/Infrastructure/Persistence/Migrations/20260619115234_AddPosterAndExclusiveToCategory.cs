using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPosterAndExclusiveToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_exclusive",
                schema: "content",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "poster_file_id",
                schema: "content",
                table: "categories",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_is_exclusive",
                schema: "content",
                table: "categories",
                column: "is_exclusive",
                unique: true,
                filter: "is_exclusive = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categories_is_exclusive",
                schema: "content",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "is_exclusive",
                schema: "content",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "poster_file_id",
                schema: "content",
                table: "categories");
        }
    }
}
