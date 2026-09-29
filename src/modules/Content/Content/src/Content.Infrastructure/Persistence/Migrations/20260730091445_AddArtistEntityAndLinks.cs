using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistEntityAndLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "artist_id",
                schema: "content",
                table: "videos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "album_id",
                schema: "content",
                table: "lyrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "artist_id",
                schema: "content",
                table: "lyrics",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "artists",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    bio = table.Column<string>(type: "text", nullable: true),
                    avatar_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    verified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_artists", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "albums",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    artist_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cover_image_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    release_year = table.Column<short>(type: "smallint", nullable: true),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_albums", x => x.id);
                    table.ForeignKey(
                        name: "fk_albums_artists_artist_id",
                        column: x => x.artist_id,
                        principalSchema: "content",
                        principalTable: "artists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_videos_artist_id",
                schema: "content",
                table: "videos",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_album_id",
                schema: "content",
                table: "lyrics",
                column: "album_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_artist_id",
                schema: "content",
                table: "lyrics",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_albums_artist_id",
                schema: "content",
                table: "albums",
                column: "artist_id");

            migrationBuilder.CreateIndex(
                name: "ix_artists_slug",
                schema: "content",
                table: "artists",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_artists_user_id",
                schema: "content",
                table: "artists",
                column: "user_id",
                unique: true,
                filter: "user_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "fk_lyrics_albums_album_id",
                schema: "content",
                table: "lyrics",
                column: "album_id",
                principalSchema: "content",
                principalTable: "albums",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_lyrics_artists_artist_id",
                schema: "content",
                table: "lyrics",
                column: "artist_id",
                principalSchema: "content",
                principalTable: "artists",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_videos_artists_artist_id",
                schema: "content",
                table: "videos",
                column: "artist_id",
                principalSchema: "content",
                principalTable: "artists",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_lyrics_albums_album_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropForeignKey(
                name: "fk_lyrics_artists_artist_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropForeignKey(
                name: "fk_videos_artists_artist_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropTable(
                name: "albums",
                schema: "content");

            migrationBuilder.DropTable(
                name: "artists",
                schema: "content");

            migrationBuilder.DropIndex(
                name: "ix_videos_artist_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropIndex(
                name: "ix_lyrics_album_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropIndex(
                name: "ix_lyrics_artist_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "artist_id",
                schema: "content",
                table: "videos");

            migrationBuilder.DropColumn(
                name: "album_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.DropColumn(
                name: "artist_id",
                schema: "content",
                table: "lyrics");
        }
    }
}
