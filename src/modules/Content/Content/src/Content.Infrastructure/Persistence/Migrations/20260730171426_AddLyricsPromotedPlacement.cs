using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLyricsPromotedPlacement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_promoted",
                schema: "content",
                table: "lyrics",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "promoted_until",
                schema: "content",
                table: "lyrics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "unpromoted_at",
                schema: "content",
                table: "lyrics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unpromoted_by",
                schema: "content",
                table: "lyrics",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unpromoted_reason",
                schema: "content",
                table: "lyrics",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_default_for_lyrics",
                schema: "content",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_categories_is_default_for_lyrics",
                schema: "content",
                table: "categories",
                column: "is_default_for_lyrics",
                unique: true,
                filter: "is_default_for_lyrics = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categories_is_default_for_lyrics",
                schema: "content",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "is_promoted",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "promoted_until",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "unpromoted_at",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "unpromoted_by",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "unpromoted_reason",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "is_default_for_lyrics",
                schema: "content",
                table: "categories");
        }
    }
}
