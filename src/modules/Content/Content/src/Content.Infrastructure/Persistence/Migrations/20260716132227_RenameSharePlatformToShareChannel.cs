using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameSharePlatformToShareChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "platform",
                schema: "content",
                table: "video_shares",
                newName: "share_channel");

            migrationBuilder.RenameColumn(
                name: "platform",
                schema: "content",
                table: "short_video_shares",
                newName: "share_channel");

            migrationBuilder.RenameColumn(
                name: "platform",
                schema: "content",
                table: "article_shares",
                newName: "share_channel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "share_channel",
                schema: "content",
                table: "video_shares",
                newName: "platform");

            migrationBuilder.RenameColumn(
                name: "share_channel",
                schema: "content",
                table: "short_video_shares",
                newName: "platform");

            migrationBuilder.RenameColumn(
                name: "share_channel",
                schema: "content",
                table: "article_shares",
                newName: "platform");
        }
    }
}
