using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistPageFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_videos_artist_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropIndex(
                name: "ix_lyrics_artist_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropIndex(
                name: "ix_albums_artist_id",
                schema: "content",
                table: "albums");

            migrationBuilder.AddColumn<List<string>>(
                name: "aliases",
                schema: "content",
                table: "artists",
                type: "text[]",
                nullable: false,
                defaultValueSql: "'{}'::text[]");

            migrationBuilder.AddColumn<DateOnly>(
                name: "birthdate",
                schema: "content",
                table: "artists",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hometown",
                schema: "content",
                table: "artists",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "initial_letter",
                schema: "content",
                table: "artists",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name_folded",
                schema: "content",
                table: "artists",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "real_name",
                schema: "content",
                table: "artists",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "release_type",
                schema: "content",
                table: "albums",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "article_artists",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    artist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_artists", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_artists_articles_article_id",
                        column: x => x.article_id,
                        principalSchema: "content",
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_article_artists_artists_artist_id",
                        column: x => x.artist_id,
                        principalSchema: "content",
                        principalTable: "artists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "artist_social_links",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    artist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artist_social_links", x => x.id);
                    table.ForeignKey(
                        name: "fk_artist_social_links_artists_artist_id",
                        column: x => x.artist_id,
                        principalSchema: "content",
                        principalTable: "artists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_videos_artist_id_status",
                schema: "content",
                table: "videos",
                columns: new[] { "artist_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_artist_id_status",
                schema: "content",
                table: "lyrics",
                columns: new[] { "artist_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_artists_initial_letter_name_folded",
                schema: "content",
                table: "artists",
                columns: new[] { "initial_letter", "name_folded" });

            migrationBuilder.CreateIndex(
                name: "ix_artists_name_folded",
                schema: "content",
                table: "artists",
                column: "name_folded");

            migrationBuilder.CreateIndex(
                name: "ix_articles_status",
                schema: "content",
                table: "articles",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_albums_artist_id_release_type",
                schema: "content",
                table: "albums",
                columns: new[] { "artist_id", "release_type" });

            migrationBuilder.CreateIndex(
                name: "ix_article_artists_article_id_artist_id",
                schema: "content",
                table: "article_artists",
                columns: new[] { "article_id", "artist_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_article_artists_artist_id",
                schema: "content",
                table: "article_artists",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_artist_social_links_artist_id_platform",
                schema: "content",
                table: "artist_social_links",
                columns: new[] { "artist_id", "platform" },
                unique: true);

            // Backfill the derived directory columns for rows that predate them. The domain
            // maintains both from this point on; the translate() list covers the accented
            // characters that occur in Latin-script names. Rows whose folded name starts with
            // a non-letter bucket under '#', matching the domain's folding rule.
            migrationBuilder.Sql(
                """
                UPDATE content.artists
                SET name_folded = upper(
                        regexp_replace(
                            translate(
                                trim(name),
                                'àáâãäåçèéêëìíîïñòóôõöùúûüýÿÀÁÂÃÄÅÇÈÉÊËÌÍÎÏÑÒÓÔÕÖÙÚÛÜÝŸ',
                                'aaaaaaceeeeiiiinooooouuuuyyAAAAAACEEEEIIIINOOOOOUUUUYY'
                            ),
                            '\s+', ' ', 'g'
                        )
                    );

                UPDATE content.artists
                SET initial_letter = CASE
                        WHEN name_folded ~ '^[A-Z]' THEN left(name_folded, 1)
                        ELSE '#'
                    END;
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_artists",
                schema: "content");

            migrationBuilder.DropTable(
                name: "artist_social_links",
                schema: "content");

            migrationBuilder.DropIndex(
                name: "ix_videos_artist_id_status",
                schema: "content",
                table: "videos");

            migrationBuilder.DropIndex(
                name: "ix_lyrics_artist_id_status",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropIndex(
                name: "ix_artists_initial_letter_name_folded",
                schema: "content",
                table: "artists");

            migrationBuilder.DropIndex(
                name: "ix_artists_name_folded",
                schema: "content",
                table: "artists");

            migrationBuilder.DropIndex(
                name: "ix_articles_status",
                schema: "content",
                table: "articles");

            migrationBuilder.DropIndex(
                name: "ix_albums_artist_id_release_type",
                schema: "content",
                table: "albums");

            migrationBuilder.DropColumn(
                name: "aliases",
                schema: "content",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "birthdate",
                schema: "content",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "hometown",
                schema: "content",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "initial_letter",
                schema: "content",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "name_folded",
                schema: "content",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "real_name",
                schema: "content",
                table: "artists");

            migrationBuilder.DropColumn(
                name: "release_type",
                schema: "content",
                table: "albums");

            migrationBuilder.CreateIndex(
                name: "ix_videos_artist_id",
                schema: "content",
                table: "videos",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_artist_id",
                schema: "content",
                table: "lyrics",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_albums_artist_id",
                schema: "content",
                table: "albums",
                column: "artist_id");
        }
    }
}
