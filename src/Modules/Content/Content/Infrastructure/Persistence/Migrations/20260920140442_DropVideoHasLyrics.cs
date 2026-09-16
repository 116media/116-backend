using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropVideoHasLyrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_lyrics",
                schema: "content",
                table: "videos");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_video_id_published",
                schema: "content",
                table: "lyrics",
                column: "video_id",
                filter: "status = 'Published'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_lyrics_video_id_published",
                schema: "content",
                table: "lyrics");

            migrationBuilder.AddColumn<bool>(
                name: "has_lyrics",
                schema: "content",
                table: "videos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
