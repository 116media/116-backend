using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntroducePromotionFieldsAndForceUnpromoteAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_featured",
                schema: "content",
                table: "videos",
                newName: "is_promoted");

            migrationBuilder.RenameColumn(
                name: "featured_until",
                schema: "content",
                table: "videos",
                newName: "unpromoted_at");

            migrationBuilder.RenameColumn(
                name: "is_featured",
                schema: "content",
                table: "articles",
                newName: "is_promoted");

            migrationBuilder.RenameColumn(
                name: "featured_until",
                schema: "content",
                table: "articles",
                newName: "unpromoted_at");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "promoted_until",
                schema: "content",
                table: "videos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unpromoted_by",
                schema: "content",
                table: "videos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unpromoted_reason",
                schema: "content",
                table: "videos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "promoted_until",
                schema: "content",
                table: "articles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unpromoted_by",
                schema: "content",
                table: "articles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unpromoted_reason",
                schema: "content",
                table: "articles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "promoted_until",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "unpromoted_by",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "unpromoted_reason",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "promoted_until",
                schema: "content",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "unpromoted_by",
                schema: "content",
                table: "articles");

            migrationBuilder.DropColumn(
                name: "unpromoted_reason",
                schema: "content",
                table: "articles");

            migrationBuilder.RenameColumn(
                name: "unpromoted_at",
                schema: "content",
                table: "videos",
                newName: "featured_until");

            migrationBuilder.RenameColumn(
                name: "is_promoted",
                schema: "content",
                table: "videos",
                newName: "is_featured");

            migrationBuilder.RenameColumn(
                name: "unpromoted_at",
                schema: "content",
                table: "articles",
                newName: "featured_until");

            migrationBuilder.RenameColumn(
                name: "is_promoted",
                schema: "content",
                table: "articles",
                newName: "is_featured");
        }
    }
}
