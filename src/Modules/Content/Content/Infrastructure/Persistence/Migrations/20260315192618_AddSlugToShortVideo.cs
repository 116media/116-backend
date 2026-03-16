using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToShortVideo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "content",
                table: "videos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "slug",
                schema: "content",
                table: "short_videos",
                type: "character varying(220)",
                maxLength: 220,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "content",
                table: "articles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "ix_videos_title",
                schema: "content",
                table: "videos",
                column: "title",
                unique: true);

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
                name: "ix_categories_name",
                schema: "content",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_articles_title",
                schema: "content",
                table: "articles",
                column: "title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_videos_title",
                schema: "content",
                table: "videos");

            migrationBuilder.DropIndex(
                name: "ix_short_videos_slug",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropIndex(
                name: "ix_short_videos_title",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropIndex(
                name: "ix_categories_name",
                schema: "content",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_articles_title",
                schema: "content",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "slug",
                schema: "content",
                table: "short_videos");

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "content",
                table: "videos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "title",
                schema: "content",
                table: "articles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
