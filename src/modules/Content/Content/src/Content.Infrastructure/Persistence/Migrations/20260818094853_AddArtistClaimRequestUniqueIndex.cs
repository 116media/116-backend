using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddArtistClaimRequestUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_artist_claim_requests_artist_id_user_id",
                schema: "content",
                table: "artist_claim_requests",
                columns: new[] { "artist_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_artist_claim_requests_artist_id_user_id",
                schema: "content",
                table: "artist_claim_requests");
        }
    }
}
