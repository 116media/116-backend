using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Replaces the per-table search vectors and their views with one projection table
    /// holding every searchable type, so a search is one query and the per-scope counts one
    /// aggregate. Backfills from the source tables, then drops what it supersedes.
    /// </summary>
    public partial class AddSearchDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "search_documents",
                schema: "content",
                columns: table => new
                {
                    type = table.Column<string>(type: "text", nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false),
                    is_visible = table.Column<bool>(type: "boolean", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    summary = table.Column<string>(type: "text", nullable: false),
                    keywords = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    image_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    rating_average = table.Column<decimal>(type: "numeric", nullable: true),
                    search_vector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_search_documents", x => new { x.type, x.entity_id });
                }
            );

            // EF can only scaffold a plain column; the vector is derived from the four text
            // columns, so the database owns it and no writer ever sends it.
            migrationBuilder.Sql("ALTER TABLE content.search_documents DROP COLUMN search_vector;");

            migrationBuilder.Sql(
                """
                ALTER TABLE content.search_documents
                    ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(summary, ''))), 'B') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(keywords, ''))), 'C') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(body, ''))), 'D')
                    ) STORED;
                """
            );

            // Every search filters on visibility, so the posting lists carry only reachable rows.
            migrationBuilder.Sql(
                """
                CREATE INDEX ix_search_documents_vector_visible
                    ON content.search_documents USING GIN (search_vector)
                    WHERE is_visible;
                """
            );

            BackfillArticles(migrationBuilder);
            BackfillVideos(migrationBuilder);
            BackfillShortVideos(migrationBuilder);

            DropSupersededObjects(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            RestoreSupersededObjects(migrationBuilder);

            migrationBuilder.DropTable(name: "search_documents", schema: "content");
        }

        private static void BackfillArticles(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(
                """
                INSERT INTO content.search_documents
                    (type, entity_id, slug, is_visible, title, summary, keywords, body,
                     image_file_id, category_id, published_at, rating_average)
                SELECT 'Article', id, slug, status = 'Published', coalesce(title, ''),
                       coalesce(headline, ''),
                       btrim(concat_ws(' ', meta_title, meta_description)),
                       coalesce(body, ''), cover_image_file_id, category_id, published_at, NULL
                FROM content.articles;
                """
            );

        private static void BackfillVideos(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(
                """
                INSERT INTO content.search_documents
                    (type, entity_id, slug, is_visible, title, summary, keywords, body,
                     image_file_id, category_id, published_at, rating_average)
                SELECT 'Video', id, slug, status = 'Published', coalesce(title, ''),
                       coalesce(description, ''),
                       btrim(concat_ws(' ', meta_title, meta_description)),
                       '', thumbnail_file_id, category_id, published_at, rating_average
                FROM content.videos;
                """
            );

        private static void BackfillShortVideos(MigrationBuilder migrationBuilder) =>
            migrationBuilder.Sql(
                """
                INSERT INTO content.search_documents
                    (type, entity_id, slug, is_visible, title, summary, keywords, body,
                     image_file_id, category_id, published_at, rating_average)
                SELECT 'Short', id, slug, is_active, coalesce(title, ''), '', '', '',
                       thumbnail_file_id, NULL, created_at, NULL
                FROM content.short_videos;
                """
            );

        private static void DropSupersededObjects(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS content.short_video_search;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS content.video_search;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS content.article_search;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_short_videos_search_vector_active;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_videos_search_vector_published;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_articles_search_vector_published;");
            migrationBuilder.Sql("ALTER TABLE content.short_videos DROP COLUMN IF EXISTS search_vector;");
            migrationBuilder.Sql("ALTER TABLE content.videos DROP COLUMN IF EXISTS search_vector;");
            migrationBuilder.Sql("ALTER TABLE content.articles DROP COLUMN IF EXISTS search_vector;");
        }

        private static void RestoreSupersededObjects(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE content.articles
                    ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(headline, ''))), 'B') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_title, ''))), 'C') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_description, ''))), 'C') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(body, ''))), 'D')
                    ) STORED;
                """
            );

            migrationBuilder.Sql(
                """
                ALTER TABLE content.videos
                    ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(description, ''))), 'B') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_title, ''))), 'C') ||
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(meta_description, ''))), 'C')
                    ) STORED;
                """
            );

            migrationBuilder.Sql(
                """
                ALTER TABLE content.short_videos
                    ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A')
                    ) STORED;
                """
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX ix_articles_search_vector_published
                    ON content.articles USING GIN (search_vector)
                    WHERE status = 'Published';
                """
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX ix_videos_search_vector_published
                    ON content.videos USING GIN (search_vector)
                    WHERE status = 'Published';
                """
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX ix_short_videos_search_vector_active
                    ON content.short_videos USING GIN (search_vector)
                    WHERE is_active;
                """
            );

            migrationBuilder.Sql(
                """
                CREATE VIEW content.article_search AS
                SELECT id, slug, title, headline, category_id, status, published_at,
                       cover_image_file_id, search_vector
                FROM content.articles;
                """
            );

            migrationBuilder.Sql(
                """
                CREATE VIEW content.video_search AS
                SELECT id, slug, title, description, category_id, status, published_at,
                       rating_average, thumbnail_file_id, search_vector
                FROM content.videos;
                """
            );

            migrationBuilder.Sql(
                """
                CREATE VIEW content.short_video_search AS
                SELECT id, slug, title, is_active, created_at, thumbnail_file_id, search_vector
                FROM content.short_videos;
                """
            );
        }
    }
}
