using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLyricsTranslationsAndReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lyrics_translations",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lyrics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "text", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_translations", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_translations_lyrics_lyrics_id",
                        column: x => x.lyrics_id,
                        principalSchema: "content",
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyrics_translation_revisions",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    translation_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_lyrics_translation_revisions", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_translation_revisions_lyrics_translations_translatio",
                        column: x => x.translation_id,
                        principalSchema: "content",
                        principalTable: "lyrics_translations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lyrics_translation_votes",
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
                    table.PrimaryKey("pk_lyrics_translation_votes", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_translation_votes_lyrics_translation_revisions_revis",
                        column: x => x.revision_id,
                        principalSchema: "content",
                        principalTable: "lyrics_translation_revisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_translation_revisions_translation_id",
                schema: "content",
                table: "lyrics_translation_revisions",
                column: "translation_id");

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_translation_votes_revision_id_user_id",
                schema: "content",
                table: "lyrics_translation_votes",
                columns: new[] { "revision_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_translations_lyrics_id_language",
                schema: "content",
                table: "lyrics_translations",
                columns: new[] { "lyrics_id", "language" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lyrics_translation_votes",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics_translation_revisions",
                schema: "content");

            migrationBuilder.DropTable(
                name: "lyrics_translations",
                schema: "content");
        }
    }
}
