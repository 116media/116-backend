using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLyricsInteractions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "like_count",
                schema: "content",
                table: "lyrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "share_count",
                schema: "content",
                table: "lyrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "view_count",
                schema: "content",
                table: "lyrics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "lyrics_likes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lyrics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_likes", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_likes_lyrics_lyrics_id",
                        column: x => x.lyrics_id,
                        principalSchema: "content",
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyrics_shares",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lyrics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    share_channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_shares", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_shares_lyrics_lyrics_id",
                        column: x => x.lyrics_id,
                        principalSchema: "content",
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyrics_view_events",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lyrics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dedup_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_counted = table.Column<bool>(type: "boolean", nullable: false),
                    dwell_ms = table.Column<int>(type: "integer", nullable: false),
                    scroll_depth_ratio = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_view_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_view_events_lyrics_lyrics_id",
                        column: x => x.lyrics_id,
                        principalSchema: "content",
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_likes_lyrics_id",
                schema: "content",
                table: "lyrics_likes",
                column: "lyrics_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_likes_user_id_lyrics_id",
                schema: "content",
                table: "lyrics_likes",
                columns: new[] { "user_id", "lyrics_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_shares_lyrics_id",
                schema: "content",
                table: "lyrics_shares",
                column: "lyrics_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_shares_user_created_lyrics",
                schema: "content",
                table: "lyrics_shares",
                columns: new[] { "user_id", "created_at", "lyrics_id" },
                descending: new[] { false, true, false },
                filter: "user_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_view_events_lyrics_id_dedup_key_created_at",
                schema: "content",
                table: "lyrics_view_events",
                columns: new[] { "lyrics_id", "dedup_key", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lyrics_likes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics_shares",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics_view_events",
                schema: "content");

            migrationBuilder.DropColumn(
                name: "like_count",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "share_count",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "view_count",
                schema: "content",
                table: "lyrics");
        }
    }
}
