using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProviderSubjectId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "provider_subject_id",
                schema: "identity",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_auth_provider_provider_subject_id",
                schema: "identity",
                table: "users",
                columns: new[] { "auth_provider", "provider_subject_id" },
                unique: true,
                filter: "provider_subject_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_auth_provider_provider_subject_id",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "provider_subject_id",
                schema: "identity",
                table: "users");
        }
    }
}
