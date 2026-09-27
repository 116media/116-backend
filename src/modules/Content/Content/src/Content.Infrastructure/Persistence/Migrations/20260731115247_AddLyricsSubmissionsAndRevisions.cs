using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLyricsSubmissionsAndRevisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lyrics_revisions",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lyrics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    proposed_text = table.Column<string>(type: "text", nullable: false),
                    edit_summary = table.Column<string>(type: "text", nullable: true),
                    proposed_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    decided_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_revisions_lyrics_lyrics_id",
                        column: x => x.lyrics_id,
                        principalSchema: "content",
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyrics_submissions",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    song_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    artist_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    lyrics_text = table.Column<string>(type: "text", nullable: false),
                    language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    submitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_note = table.Column<string>(type: "text", nullable: true),
                    published_lyrics_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_submissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lyrics_revision_votes",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    revision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vote = table.Column<string>(type: "text", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_revision_votes", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_revision_votes_lyrics_revisions_revision_id",
                        column: x => x.revision_id,
                        principalSchema: "content",
                        principalTable: "lyrics_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_revision_votes_revision_id_user_id",
                schema: "content",
                table: "lyrics_revision_votes",
                columns: new[] { "revision_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_revisions_lyrics_id",
                schema: "content",
                table: "lyrics_revisions",
                column: "lyrics_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lyrics_revision_votes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics_submissions",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics_revisions",
                schema: "content");
        }
    }
}
