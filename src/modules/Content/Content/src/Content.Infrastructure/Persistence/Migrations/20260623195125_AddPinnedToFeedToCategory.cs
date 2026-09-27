using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPinnedToFeedToCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "pinned_to_feed_at",
                schema: "content",
                table: "categories",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_pinned_to_feed_at",
                schema: "content",
                table: "categories",
                column: "pinned_to_feed_at",
                filter: "pinned_to_feed_at IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categories_pinned_to_feed_at",
                schema: "content",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "pinned_to_feed_at",
                schema: "content",
                table: "categories");
        }
    }
}
