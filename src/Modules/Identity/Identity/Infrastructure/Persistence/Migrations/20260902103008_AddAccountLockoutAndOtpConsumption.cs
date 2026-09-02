using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLockoutAndOtpConsumption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "failed_login_attempts",
                schema: "identity",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_until",
                schema: "identity",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "consumed_at",
                schema: "identity",
                table: "otps",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "user_otp_state",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_otp_state", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_otp_state_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Outstanding codes were hashed with PBKDF2 and cannot be verified by the keyed
            // hasher that replaces it. They expire within the hour regardless.
            migrationBuilder.Sql("DELETE FROM identity.otps;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_otp_state",
                schema: "identity");

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "locked_until",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "consumed_at",
                schema: "identity",
                table: "otps");
        }
    }
}
