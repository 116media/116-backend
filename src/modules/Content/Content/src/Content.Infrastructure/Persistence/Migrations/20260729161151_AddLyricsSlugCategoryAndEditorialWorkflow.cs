using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLyricsSlugCategoryAndEditorialWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                schema: "content",
                table: "lyrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "customer_id",
                schema: "content",
                table: "lyrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "order_item_id",
                schema: "content",
                table: "lyrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "published_at",
                schema: "content",
                table: "lyrics",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                schema: "content",
                table: "lyrics",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "content",
                table: "lyrics",
                type: "character varying(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "content",
                table: "lyrics",
                type: "text",
                nullable: false,
                defaultValue: "Draft");

            // Backfill existing rows before the FK/unique-index constraints below are applied.
            // category_id: every existing row gets the first available category (arbitrary but
            // valid) — an admin can recategorize afterwards. No-op when content.lyrics is empty,
            // and also a no-op (leaving the placeholder default) when content.categories has no
            // rows yet, matching the "pre-production scaffold data" assumption for this table.
            migrationBuilder.Sql(
                "UPDATE content.lyrics "
                    + "SET category_id = (SELECT id FROM content.categories ORDER BY id LIMIT 1) "
                    + "WHERE category_id = '00000000-0000-0000-0000-000000000000' "
                    + "AND EXISTS (SELECT 1 FROM content.categories);"
            );

            // slug: derive a unique, URL-safe slug from song_title/artist_name, suffixed with the
            // row id to guarantee uniqueness ahead of the unique index created below.
            migrationBuilder.Sql(
                "UPDATE content.lyrics "
                    + "SET slug = trim(both '-' from "
                    + "regexp_replace(lower(song_title || '-' || artist_name), '[^a-z0-9]+', '-', 'g')) "
                    + "|| '-' || substr(id::text, 1, 8) "
                    + "WHERE slug = '';"
            );

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_category_id",
                schema: "content",
                table: "lyrics",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_customer_id",
                schema: "content",
                table: "lyrics",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_slug",
                schema: "content",
                table: "lyrics",
                column: "slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_lyrics_categories_category_id",
                schema: "content",
                table: "lyrics",
                column: "category_id",
                principalSchema: "content",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_lyrics_customers_customer_id",
                schema: "content",
                table: "lyrics",
                column: "customer_id",
                principalSchema: "content",
                principalTable: "customers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_lyrics_categories_category_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropForeignKey(
                name: "fk_lyrics_customers_customer_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropIndex(
                name: "ix_lyrics_category_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropIndex(
                name: "ix_lyrics_customer_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropIndex(
                name: "ix_lyrics_slug",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "category_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "customer_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "order_item_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "published_at",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "rejection_reason",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "content",
                table: "lyrics");
        }
    }
}
