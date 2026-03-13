using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEditorialEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "articles",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    headline = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, defaultValue: ""),
                    body = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    author_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    social_boost = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    featured_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    meta_title = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    meta_description = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    like_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    comment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    share_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bookmark_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_articles", x => x.id);
                    table.ForeignKey(
                        name: "fk_articles_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "content",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_articles_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "content",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "videos",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    thumbnail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    thumbnail_storage_key = table.Column<string>(type: "text", nullable: true),
                    youtube_video_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    social_boost = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    featured_until = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    has_lyrics = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    shooting_scheduled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    meta_title = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    meta_description = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    rating_average = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false, defaultValue: 0m),
                    rating_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    share_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_videos_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "content",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_videos_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "content",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "article_images",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    image_type = table.Column<string>(type: "text", nullable: false, defaultValue: "Body"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_images_articles_article_id",
                        column: x => x.article_id,
                        principalSchema: "content",
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_tags",
                schema: "content",
                columns: table => new
                {
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_tags", x => new { x.article_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_article_tags_articles_article_id",
                        column: x => x.article_id,
                        principalSchema: "content",
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_article_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "content",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyrics",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_id = table.Column<Guid>(type: "uuid", nullable: true),
                    article_id = table.Column<Guid>(type: "uuid", nullable: true),
                    song_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    artist_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    lyrics_text = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "fr"),
                    meta_title = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    meta_description = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    meta_keywords = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    structured_data = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_articles_article_id",
                        column: x => x.article_id,
                        principalSchema: "content",
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_lyrics_videos_video_id",
                        column: x => x.video_id,
                        principalSchema: "content",
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "short_videos",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    video_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    video_storage_key = table.Column<string>(type: "text", nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    thumbnail_storage_key = table.Column<string>(type: "text", nullable: true),
                    video_id = table.Column<Guid>(type: "uuid", nullable: true),
                    has_full_video = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    view_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    like_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    share_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bookmark_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_short_videos_videos_video_id",
                        column: x => x.video_id,
                        principalSchema: "content",
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "video_tags",
                schema: "content",
                columns: table => new
                {
                    video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_video_tags", x => new { x.video_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_video_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "content",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_video_tags_videos_video_id",
                        column: x => x.video_id,
                        principalSchema: "content",
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_images_article_id",
                schema: "content",
                table: "article_images",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_tags_tag_id",
                schema: "content",
                table: "article_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_articles_category_id",
                schema: "content",
                table: "articles",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_articles_customer_id",
                schema: "content",
                table: "articles",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_articles_slug",
                schema: "content",
                table: "articles",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_article_id",
                schema: "content",
                table: "lyrics",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_video_id",
                schema: "content",
                table: "lyrics",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "ix_short_videos_video_id",
                schema: "content",
                table: "short_videos",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "ix_video_tags_tag_id",
                schema: "content",
                table: "video_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_videos_category_id",
                schema: "content",
                table: "videos",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_videos_customer_id",
                schema: "content",
                table: "videos",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_videos_slug",
                schema: "content",
                table: "videos",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_images",
                schema: "content");

            migrationBuilder.DropTable(
                name: "article_tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics",
                schema: "content");

            migrationBuilder.DropTable(
                name: "short_videos",
                schema: "content");

            migrationBuilder.DropTable(
                name: "video_tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "articles",
                schema: "content");

            migrationBuilder.DropTable(
                name: "videos",
                schema: "content");
        }
    }
}
