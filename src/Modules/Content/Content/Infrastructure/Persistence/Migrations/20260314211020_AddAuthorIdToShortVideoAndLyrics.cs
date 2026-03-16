using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthorIdToShortVideoAndLyrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "author_id",
                schema: "content",
                table: "videos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);

            migrationBuilder.AddColumn<Guid>(
                name: "author_id",
                schema: "content",
                table: "short_videos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "author_id",
                schema: "content",
                table: "lyrics",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "author_id",
                schema: "content",
                table: "articles",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(36)",
                oldMaxLength: 36);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "author_id",
                schema: "content",
                table: "short_videos");

            migrationBuilder.DropColumn(
                name: "author_id",
                schema: "content",
                table: "lyrics");

            migrationBuilder.AlterColumn<string>(
                name: "author_id",
                schema: "content",
                table: "videos",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "author_id",
                schema: "content",
                table: "articles",
                type: "character varying(36)",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
