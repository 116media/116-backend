using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStreamingLinksAndAlbumLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "streaming_links",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    album_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lyrics_id = table.Column<Guid>(type: "uuid", nullable: true),
                    platform = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_streaming_links", x => x.id);
                    table.CheckConstraint("ck_streaming_links_exactly_one_target", "(album_id IS NOT NULL AND lyrics_id IS NULL) OR (album_id IS NULL AND lyrics_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "fk_streaming_links_albums_album_id",
                        column: x => x.album_id,
                        principalSchema: "content",
                        principalTable: "albums",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_streaming_links_lyrics_lyrics_id",
                        column: x => x.lyrics_id,
                        principalSchema: "content",
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_streaming_links_album_id_platform",
                schema: "content",
                table: "streaming_links",
                columns: new[] { "album_id", "platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_streaming_links_lyrics_id_platform",
                schema: "content",
                table: "streaming_links",
                columns: new[] { "lyrics_id", "platform" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "streaming_links",
                schema: "content");
        }
    }
}
