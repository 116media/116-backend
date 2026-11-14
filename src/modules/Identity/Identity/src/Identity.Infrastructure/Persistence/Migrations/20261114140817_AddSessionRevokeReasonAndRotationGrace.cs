using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionRevokeReasonAndRotationGrace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "previous_refresh_token_hash",
                schema: "identity",
                table: "sessions",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "previous_refresh_token_usable_until",
                schema: "identity",
                table: "sessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "revoke_reason",
                schema: "identity",
                table: "sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_sessions_previous_refresh_token_hash",
                schema: "identity",
                table: "sessions",
                column: "previous_refresh_token_hash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sessions_previous_refresh_token_hash",
                schema: "identity",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "previous_refresh_token_hash",
                schema: "identity",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "previous_refresh_token_usable_until",
                schema: "identity",
                table: "sessions");

            migrationBuilder.DropColumn(
                name: "revoke_reason",
                schema: "identity",
                table: "sessions");
        }
    }
}
