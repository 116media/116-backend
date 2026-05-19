using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateYoutubeIdToFullUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "youtube_video_id",
                schema: "content",
                table: "videos");

            migrationBuilder.AddColumn<string>(
                name: "youtube_video_url",
                schema: "content",
                table: "videos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "youtube_video_url",
                schema: "content",
                table: "videos");

            migrationBuilder.AddColumn<string>(
                name: "youtube_video_id",
                schema: "content",
                table: "videos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }
    }
}
