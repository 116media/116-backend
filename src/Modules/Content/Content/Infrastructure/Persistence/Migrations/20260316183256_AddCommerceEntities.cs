using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCommerceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "image_type",
                schema: "content",
                table: "article_images",
                type: "text",
                nullable: false,
                defaultValue: "Cover",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Body");

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
                    payment_proof_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_item_tiers",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_payments",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_order_items",
                schema: "content");

            migrationBuilder.DropTable(
                name: "content_orders",
                schema: "content");

            migrationBuilder.AlterColumn<string>(
                name: "image_type",
                schema: "content",
                table: "article_images",
                type: "text",
                nullable: false,
                defaultValue: "Body",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Cover");
        }
    }
}
