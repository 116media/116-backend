using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleCommentThreadingAndLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "like_count",
                schema: "content",
                table: "article_comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_comment_id",
                schema: "content",
                table: "article_comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "article_comment_likes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_comment_likes", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_comment_likes_article_comments_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "content",
                        principalTable: "article_comments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_comments_parent",
                schema: "content",
                table: "article_comments",
                column: "parent_comment_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_comment_likes_comment_user",
                schema: "content",
                table: "article_comment_likes",
                columns: new[] { "comment_id", "user_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_article_comments_article_comments_parent_comment_id",
                schema: "content",
                table: "article_comments",
                column: "parent_comment_id",
                principalSchema: "content",
                principalTable: "article_comments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_comments_article_comments_parent_comment_id",
                schema: "content",
                table: "article_comments");

            migrationBuilder.DropTable(
                name: "article_comment_likes",
                schema: "content");

            migrationBuilder.DropIndex(
                name: "ix_article_comments_parent",
                schema: "content",
                table: "article_comments");

            migrationBuilder.DropColumn(
                name: "like_count",
                schema: "content",
                table: "article_comments");

            migrationBuilder.DropColumn(
                name: "parent_comment_id",
                schema: "content",
                table: "article_comments");
        }
    }
}
