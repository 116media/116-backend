using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMetaKeywordsFromLyrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "meta_keywords",
                schema: "content",
                table: "lyrics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "meta_keywords",
                schema: "content",
                table: "lyrics",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }
    }
}
