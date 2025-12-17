using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarSourceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_source",
                schema: "authentication",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_source",
                schema: "authentication",
                table: "users");
        }
    }
}
