using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFileIdColumnsToContentEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "thumbnail_storage_key",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "thumbnail_url",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "thumbnail_storage_key",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "thumbnail_url",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "video_storage_key",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "video_url",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "cover_image_url",
                schema: "content",
                table: "articles");

            migrationBuilder.AddColumn<Guid>(
                name: "thumbnail_file_id",
                schema: "content",
                table: "videos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "thumbnail_file_id",
                schema: "content",
                table: "short_videos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "video_file_id",
                schema: "content",
                table: "short_videos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "cover_image_file_id",
                schema: "content",
                table: "articles",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "thumbnail_file_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "thumbnail_file_id",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "video_file_id",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "cover_image_file_id",
                schema: "content",
                table: "articles");

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_storage_key",
                schema: "content",
                table: "videos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_url",
                schema: "content",
                table: "videos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_storage_key",
                schema: "content",
                table: "short_videos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_url",
                schema: "content",
                table: "short_videos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "video_storage_key",
                schema: "content",
                table: "short_videos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "video_url",
                schema: "content",
                table: "short_videos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "cover_image_url",
                schema: "content",
                table: "articles",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
