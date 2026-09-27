using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSongMetadataAndCoverToLyrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "album",
                schema: "content",
                table: "lyrics",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cover_image_file_id",
                schema: "content",
                table: "lyrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "label",
                schema: "content",
                table: "lyrics",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "producer",
                schema: "content",
                table: "lyrics",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "release_year",
                schema: "content",
                table: "lyrics",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "songwriter",
                schema: "content",
                table: "lyrics",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "album",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "cover_image_file_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "label",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "producer",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "release_year",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "songwriter",
                schema: "content",
                table: "lyrics");
        }
    }
}
