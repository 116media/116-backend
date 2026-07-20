using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLyricsTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "lyrics_tags",
                schema: "content",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    lyrics_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lyrics_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_lyrics_tags_lyrics_lyrics_id",
                        column: x => x.lyrics_id,
                        principalSchema: "content",
                        principalTable: "lyrics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lyrics_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "content",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_tags_lyrics_id_tag_id",
                schema: "content",
                table: "lyrics_tags",
                columns: new[] { "lyrics_id", "tag_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lyrics_tags_tag_id",
                schema: "content",
                table: "lyrics_tags",
                column: "tag_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lyrics_tags",
                schema: "content");
        }
    }
}
