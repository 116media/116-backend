using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSpotPriorityAndPromotionFeedSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "promotion_level_id",
                schema: "content",
                table: "videos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "spot_priority",
                schema: "content",
                table: "promotion_levels",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_gossip_fallback",
                schema: "content",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "promotion_level_id",
                schema: "content",
                table: "articles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_videos_promotion_level_id",
                schema: "content",
                table: "videos",
                column: "promotion_level_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_promotion_levels_spot_priority",
                schema: "content",
                table: "promotion_levels",
                sql: "spot_priority IN (1, 2, 3)");

            migrationBuilder.CreateIndex(
                name: "ix_categories_is_gossip_fallback",
                schema: "content",
                table: "categories",
                column: "is_gossip_fallback",
                unique: true,
                filter: "is_gossip_fallback = true");

            migrationBuilder.CreateIndex(
                name: "ix_articles_promotion_level_id",
                schema: "content",
                table: "articles",
                column: "promotion_level_id");

            migrationBuilder.AddForeignKey(
                name: "fk_articles_promotion_levels_promotion_level_id",
                schema: "content",
                table: "articles",
                column: "promotion_level_id",
                principalSchema: "content",
                principalTable: "promotion_levels",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_videos_promotion_levels_promotion_level_id",
                schema: "content",
                table: "videos",
                column: "promotion_level_id",
                principalSchema: "content",
                principalTable: "promotion_levels",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_articles_promotion_levels_promotion_level_id",
                schema: "content",
                table: "articles");

            migrationBuilder.DropForeignKey(
                name: "fk_videos_promotion_levels_promotion_level_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropIndex(
                name: "ix_videos_promotion_level_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropCheckConstraint(
                name: "ck_promotion_levels_spot_priority",
                schema: "content",
                table: "promotion_levels");

            migrationBuilder.DropIndex(
                name: "ix_categories_is_gossip_fallback",
                schema: "content",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_articles_promotion_level_id",
                schema: "content",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "promotion_level_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "spot_priority",
                schema: "content",
                table: "promotion_levels");

            migrationBuilder.DropColumn(
                name: "is_gossip_fallback",
                schema: "content",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "promotion_level_id",
                schema: "content",
                table: "articles");
        }
    }
}
