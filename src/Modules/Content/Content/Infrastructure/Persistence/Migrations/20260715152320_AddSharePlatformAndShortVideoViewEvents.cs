using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSharePlatformAndShortVideoViewEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "platform",
                schema: "content",
                table: "video_shares",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "platform",
                schema: "content",
                table: "short_video_shares",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "platform",
                schema: "content",
                table: "article_shares",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "short_video_view_events",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    short_video_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dedup_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_counted = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_short_video_view_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_short_video_view_events_short_videos_short_video_id",
                        column: x => x.short_video_id,
                        principalSchema: "content",
                        principalTable: "short_videos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_short_video_view_events_short_video_id_dedup_key_created_at",
                schema: "content",
                table: "short_video_view_events",
                columns: new[] { "short_video_id", "dedup_key", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "short_video_view_events",
                schema: "content");

            migrationBuilder.DropColumn(
                name: "platform",
                schema: "content",
                table: "video_shares");

            migrationBuilder.DropColumn(
                name: "platform",
                schema: "content",
                table: "short_video_shares");

            migrationBuilder.DropColumn(
                name: "platform",
                schema: "content",
                table: "article_shares");
        }
    }
}
