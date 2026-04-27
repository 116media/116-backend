using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitContentSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "content_types",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    company = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "packages",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    flat_price_usd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_packages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "playlists",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_playlists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pricing_tiers",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pricing_tiers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotion_levels",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    duration_days = table.Column<int>(type: "integer", nullable: false),
                    price_usd = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_promotion_levels", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    slug = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categories",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    is_free = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_content_types_content_type_id",
                        column: x => x.content_type_id,
                        principalSchema: "content",
                        principalTable: "content_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_orders",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_id = table.Column<Guid>(type: "uuid", nullable: true),
                    total_amount_usd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalSchema: "content",
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_content_orders_packages_package_id",
                        column: x => x.package_id,
                        principalSchema: "content",
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "articles",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    headline = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, defaultValue: ""),
                    body = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
                    cover_image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "category_pricing",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pricing_tier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_usd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_pricing", x => x.id);
                    table.ForeignKey(
                        name: "fk_category_pricing_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "content",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_category_pricing_pricing_tiers_pricing_tier_id",
                        column: x => x.pricing_tier_id,
                        principalSchema: "content",
                        principalTable: "pricing_tiers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "package_slots",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    package_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package_slots", x => x.id);
                    table.ForeignKey(
                        name: "fk_package_slots_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "content",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_package_slots_packages_package_id",
                        column: x => x.package_id,
                        principalSchema: "content",
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
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
                name: "content_order_items",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_kind = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    promotion_level_id = table.Column<Guid>(type: "uuid", nullable: true),
                    promo_price_snapshot_usd = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    social_boost = table.Column<bool>(type: "boolean", nullable: false),
                    is_bonus = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_order_items_categories_category_id",
                        column: x => x.category_id,
                        principalSchema: "content",
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_content_order_items_content_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "content",
                        principalTable: "content_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_content_order_items_promotion_levels_promotion_level_id",
                        column: x => x.promotion_level_id,
                        principalSchema: "content",
                        principalTable: "promotion_levels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "content_payments",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_usd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    payment_method = table.Column<int>(type: "integer", nullable: true),
                    payment_proof_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    verified_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    receipt_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_payments", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_payments_content_orders_order_id",
                        column: x => x.order_id,
                        principalSchema: "content",
                        principalTable: "content_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_bookmarks",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_bookmarks", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_bookmarks_articles_article_id",
                        column: x => x.article_id,
                        principalSchema: "content",
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_comments",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_comments_articles_article_id",
                        column: x => x.article_id,
                        principalSchema: "content",
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                    image_type = table.Column<string>(type: "text", nullable: false, defaultValue: "Cover"),
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
                name: "article_likes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_likes", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_likes_articles_article_id",
                        column: x => x.article_id,
                        principalSchema: "content",
                        principalTable: "articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "article_shares",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_shares", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_shares_articles_article_id",
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
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    article_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_tags", x => x.id);
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
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "playlist_videos",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    playlist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_playlist_videos", x => x.id);
                    table.ForeignKey(
                        name: "fk_playlist_videos_playlists_playlist_id",
                        column: x => x.playlist_id,
                        principalSchema: "content",
                        principalTable: "playlists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_playlist_videos_videos_video_id",
                        column: x => x.video_id,
                        principalSchema: "content",
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "short_videos",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
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
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "video_ratings",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stars = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_video_ratings", x => x.id);
                    table.ForeignKey(
                        name: "fk_video_ratings_videos_video_id",
                        column: x => x.video_id,
                        principalSchema: "content",
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "video_shares",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_video_shares", x => x.id);
                    table.ForeignKey(
                        name: "fk_video_shares_videos_video_id",
                        column: x => x.video_id,
                        principalSchema: "content",
                        principalTable: "videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "video_tags",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_video_tags", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "content_item_tiers",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pricing_tier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price_snapshot_usd = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_item_tiers", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_item_tiers_content_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalSchema: "content",
                        principalTable: "content_order_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_content_item_tiers_pricing_tiers_pricing_tier_id",
                        column: x => x.pricing_tier_id,
                        principalSchema: "content",
                        principalTable: "pricing_tiers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "short_video_bookmarks",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    short_video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_video_bookmarks", x => x.id);
                    table.ForeignKey(
                        name: "fk_short_video_bookmarks_short_videos_short_video_id",
                        column: x => x.short_video_id,
                        principalSchema: "content",
                        principalTable: "short_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "short_video_likes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    short_video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_video_likes", x => x.id);
                    table.ForeignKey(
                        name: "fk_short_video_likes_short_videos_short_video_id",
                        column: x => x.short_video_id,
                        principalSchema: "content",
                        principalTable: "short_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "short_video_shares",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    short_video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_video_shares", x => x.id);
                    table.ForeignKey(
                        name: "fk_short_video_shares_short_videos_short_video_id",
                        column: x => x.short_video_id,
                        principalSchema: "content",
                        principalTable: "short_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_bookmarks_article_id",
                schema: "content",
                table: "article_bookmarks",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_bookmarks_user_id_article_id",
                schema: "content",
                table: "article_bookmarks",
                columns: new[] { "user_id", "article_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_article_comments_article",
                schema: "content",
                table: "article_comments",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_images_article_id",
                schema: "content",
                table: "article_images",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_likes_article_id",
                schema: "content",
                table: "article_likes",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_likes_user_id_article_id",
                schema: "content",
                table: "article_likes",
                columns: new[] { "user_id", "article_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_article_shares_article_id",
                schema: "content",
                table: "article_shares",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_tags_article_id_tag_id",
                schema: "content",
                table: "article_tags",
                columns: new[] { "article_id", "tag_id" },
                unique: true);

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
                name: "ix_articles_title",
                schema: "content",
                table: "articles",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_content_type_id",
                schema: "content",
                table: "categories",
                column: "content_type_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_name",
                schema: "content",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                schema: "content",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_category_pricing_pricing_tier_id",
                schema: "content",
                table: "category_pricing",
                column: "pricing_tier_id");

            migrationBuilder.CreateIndex(
                name: "uq_category_pricing_category_tier",
                schema: "content",
                table: "category_pricing",
                columns: new[] { "category_id", "pricing_tier_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_item_tiers_order_item_id",
                schema: "content",
                table: "content_item_tiers",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_item_tiers_pricing_tier_id",
                schema: "content",
                table: "content_item_tiers",
                column: "pricing_tier_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_order_items_category_id",
                schema: "content",
                table: "content_order_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_order_items_order_id",
                schema: "content",
                table: "content_order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_order_items_promotion_level_id",
                schema: "content",
                table: "content_order_items",
                column: "promotion_level_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_orders_customer_id",
                schema: "content",
                table: "content_orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_orders_package_id",
                schema: "content",
                table: "content_orders",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_payments_order_id",
                schema: "content",
                table: "content_payments",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_content_types_name",
                schema: "content",
                table: "content_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_customers_email",
                schema: "content",
                table: "customers",
                column: "email",
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
                name: "ix_package_slots_category_id",
                schema: "content",
                table: "package_slots",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_package_slots_package_id",
                schema: "content",
                table: "package_slots",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_playlist_videos_playlist_id_video_id",
                schema: "content",
                table: "playlist_videos",
                columns: new[] { "playlist_id", "video_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_playlist_videos_video_id",
                schema: "content",
                table: "playlist_videos",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "ix_playlists_user",
                schema: "content",
                table: "playlists",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_pricing_tiers_name",
                schema: "content",
                table: "pricing_tiers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promotion_levels_name",
                schema: "content",
                table: "promotion_levels",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_video_bookmarks_short_video_id",
                schema: "content",
                table: "short_video_bookmarks",
                column: "short_video_id");

            migrationBuilder.CreateIndex(
                name: "ix_short_video_bookmarks_user_id_short_video_id",
                schema: "content",
                table: "short_video_bookmarks",
                columns: new[] { "user_id", "short_video_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_video_likes_short_video_id",
                schema: "content",
                table: "short_video_likes",
                column: "short_video_id");

            migrationBuilder.CreateIndex(
                name: "ix_short_video_likes_user_id_short_video_id",
                schema: "content",
                table: "short_video_likes",
                columns: new[] { "user_id", "short_video_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_video_shares_short_video_id",
                schema: "content",
                table: "short_video_shares",
                column: "short_video_id");

            migrationBuilder.CreateIndex(
                name: "ix_short_videos_slug",
                schema: "content",
                table: "short_videos",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_videos_title",
                schema: "content",
                table: "short_videos",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_short_videos_video_id",
                schema: "content",
                table: "short_videos",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                schema: "content",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_slug",
                schema: "content",
                table: "tags",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_video_ratings_user_video",
                schema: "content",
                table: "video_ratings",
                columns: new[] { "user_id", "video_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_video_ratings_video_id",
                schema: "content",
                table: "video_ratings",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "ix_video_shares_video_id",
                schema: "content",
                table: "video_shares",
                column: "video_id");

            migrationBuilder.CreateIndex(
                name: "ix_video_tags_tag_id",
                schema: "content",
                table: "video_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_video_tags_video_id_tag_id",
                schema: "content",
                table: "video_tags",
                columns: new[] { "video_id", "tag_id" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_videos_title",
                schema: "content",
                table: "videos",
                column: "title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_bookmarks",
                schema: "content");

            migrationBuilder.DropTable(
                name: "article_comments",
                schema: "content");

            migrationBuilder.DropTable(
                name: "article_images",
                schema: "content");

            migrationBuilder.DropTable(
                name: "article_likes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "article_shares",
                schema: "content");

            migrationBuilder.DropTable(
                name: "article_tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "category_pricing",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_item_tiers",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_payments",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics",
                schema: "content");

            migrationBuilder.DropTable(
                name: "package_slots",
                schema: "content");

            migrationBuilder.DropTable(
                name: "playlist_videos",
                schema: "content");

            migrationBuilder.DropTable(
                name: "short_video_bookmarks",
                schema: "content");

            migrationBuilder.DropTable(
                name: "short_video_likes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "short_video_shares",
                schema: "content");

            migrationBuilder.DropTable(
                name: "video_ratings",
                schema: "content");

            migrationBuilder.DropTable(
                name: "video_shares",
                schema: "content");

            migrationBuilder.DropTable(
                name: "video_tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_order_items",
                schema: "content");

            migrationBuilder.DropTable(
                name: "pricing_tiers",
                schema: "content");

            migrationBuilder.DropTable(
                name: "articles",
                schema: "content");

            migrationBuilder.DropTable(
                name: "playlists",
                schema: "content");

            migrationBuilder.DropTable(
                name: "short_videos",
                schema: "content");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_orders",
                schema: "content");

            migrationBuilder.DropTable(
                name: "promotion_levels",
                schema: "content");

            migrationBuilder.DropTable(
                name: "videos",
                schema: "content");

            migrationBuilder.DropTable(
                name: "packages",
                schema: "content");

            migrationBuilder.DropTable(
                name: "categories",
                schema: "content");

            migrationBuilder.DropTable(
                name: "customers",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_types",
                schema: "content");
        }
    }
}
