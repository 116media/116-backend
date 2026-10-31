using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Adds the full-text search vectors and their GIN indexes for articles and videos,
    /// the immutable unaccent wrapper the generated columns depend on, and the read views
    /// the search queries project from.
    /// </summary>
    public partial class AddSearchVectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS unaccent;");

            // The dictionary is named explicitly so the call stops resolving it per row,
            // which is what makes the one-argument form STABLE and unusable here.
            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION content.immutable_unaccent(text)
                RETURNS text LANGUAGE sql IMMUTABLE PARALLEL SAFE STRICT
                AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
                """
            );

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS content.video_search;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS content.article_search;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_videos_search_vector_published;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_articles_search_vector_published;");
            migrationBuilder.Sql("ALTER TABLE content.videos DROP COLUMN IF EXISTS search_vector;");
            migrationBuilder.Sql("ALTER TABLE content.articles DROP COLUMN IF EXISTS search_vector;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS content.immutable_unaccent(text);");
        }
    }
}
