using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShortVideoFeedRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "feed_rank",
                schema: "content",
                table: "short_videos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            // Backfill existing rows with a distinct, well-spread 64-bit rank derived from the
            // id, so the unique index holds and the shuffle is uniform. New rows get a random
            // rank from the domain entity.
            migrationBuilder.Sql(
                "UPDATE content.short_videos "
                    + "SET feed_rank = ('x' || substr(md5(id::text), 1, 16))::bit(64)::bigint;"
            );

            migrationBuilder.CreateIndex(
                name: "ix_short_videos_feed_rank",
                schema: "content",
                table: "short_videos",
                column: "feed_rank",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_short_videos_feed_rank",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "feed_rank",
                schema: "content",
                table: "short_videos");
        }
    }
}
