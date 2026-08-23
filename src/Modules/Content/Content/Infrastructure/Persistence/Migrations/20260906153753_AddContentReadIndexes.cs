using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// Every index is built with <c>CONCURRENTLY</c>, which cannot run inside a transaction —
    /// hence <c>suppressTransaction: true</c> on every statement. Apply this migration out of
    /// band, not from application startup.
    /// </remarks>
    public partial class AddContentReadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_status_published_at
                    ON content.articles (status, published_at DESC);
                """,
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_category_status_published_at
                    ON content.articles (category_id, status, published_at);
                """,
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_promoted_published_at
                    ON content.articles (published_at DESC)
                    WHERE is_promoted = true;
                """,
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_videos_status_published_at
                    ON content.videos (status, published_at DESC);
                """,
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_short_video_view_events_uncounted_created_at
                    ON content.short_video_view_events (created_at)
                    WHERE is_counted = false;
                """,
                suppressTransaction: true
            );

            // Serves the case-insensitive tag lookup (LOWER(name) = LOWER(@name)); an expression
            // index has no model representation, so it exists only here.
            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_tags_name_lower
                    ON content.tags (LOWER(name));
                """,
                suppressTransaction: true
            );

            // The composite above leads with category_id, so the single-column FK index is
            // redundant; dropped concurrently to avoid the lock.
            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS content.ix_articles_category_id;",
                suppressTransaction: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_category_id
                    ON content.articles (category_id);
                """,
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS content.ix_tags_name_lower;",
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS content.ix_short_video_view_events_uncounted_created_at;",
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS content.ix_videos_status_published_at;",
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS content.ix_articles_promoted_published_at;",
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS content.ix_articles_category_status_published_at;",
                suppressTransaction: true
            );

            migrationBuilder.Sql(
                "DROP INDEX CONCURRENTLY IF EXISTS content.ix_articles_status_published_at;",
                suppressTransaction: true
            );
        }
    }
}
