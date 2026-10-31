using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Adds the full-text search vector and its GIN index for short videos, plus the read
    /// view the search query projects from. A short carries only a title, so its vector has
    /// a single weight, and the index covers the active rows the public feed can reach.
    /// </summary>
    public partial class AddShortVideoSearchVector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE content.short_videos
                    ADD COLUMN search_vector tsvector
                    GENERATED ALWAYS AS (
                        setweight(to_tsvector('french', content.immutable_unaccent(coalesce(title, ''))), 'A')
                    ) STORED;
                """
            );

            // Shorts have no editorial status; is_active alone is their public visibility.
            migrationBuilder.Sql(
                """
                CREATE INDEX ix_short_videos_search_vector_active
                    ON content.short_videos USING GIN (search_vector)
                    WHERE is_active;
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS content.short_video_search;");
            migrationBuilder.Sql("DROP INDEX IF EXISTS content.ix_short_videos_search_vector_active;");
            migrationBuilder.Sql("ALTER TABLE content.short_videos DROP COLUMN IF EXISTS search_vector;");
        }
    }
}
