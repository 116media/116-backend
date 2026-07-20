using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteCollectionReadIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_video_shares_user_created_video",
                schema: "content",
                table: "video_shares",
                columns: new[] { "user_id", "created_at", "video_id" },
                descending: new[] { false, true, false },
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_short_video_shares_user_created_short",
                schema: "content",
                table: "short_video_shares",
                columns: new[] { "user_id", "created_at", "short_video_id" },
                descending: new[] { false, true, false },
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_article_shares_user_created_article",
                schema: "content",
                table: "article_shares",
                columns: new[] { "user_id", "created_at", "article_id" },
                descending: new[] { false, true, false },
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_article_comments_user_deleted_created_article",
                schema: "content",
                table: "article_comments",
                columns: new[] { "user_id", "is_deleted", "created_at", "article_id" },
                descending: new[] { false, false, true, false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_video_shares_user_created_video",
                schema: "content",
                table: "video_shares");

            migrationBuilder.DropIndex(
                name: "ix_short_video_shares_user_created_short",
                schema: "content",
                table: "short_video_shares");

            migrationBuilder.DropIndex(
                name: "ix_article_shares_user_created_article",
                schema: "content",
                table: "article_shares");

            migrationBuilder.DropIndex(
                name: "ix_article_comments_user_deleted_created_article",
                schema: "content",
                table: "article_comments");
        }
    }
}
