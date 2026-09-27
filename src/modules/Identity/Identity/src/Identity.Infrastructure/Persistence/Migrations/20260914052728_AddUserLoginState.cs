using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLoginState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_login_state",
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
                    table.PrimaryKey("pk_user_login_state", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_login_state_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Carry the counters over before the columns are dropped; accounts with nothing
            // recorded stay rowless until their first failure provisions one.
            migrationBuilder.Sql(
                """
                INSERT INTO identity.user_login_state (user_id, failed_attempts, locked_until, created_at)
                SELECT id, failed_login_attempts, locked_until, now()
                FROM identity.users
                WHERE failed_login_attempts <> 0 OR locked_until IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "failed_login_attempts",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "locked_until",
                schema: "identity",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            // Return the counters to the user row before the table goes.
            migrationBuilder.Sql(
                """
                UPDATE identity.users u
                SET failed_login_attempts = s.failed_attempts,
                    locked_until = s.locked_until
                FROM identity.user_login_state s
                WHERE s.user_id = u.id;
                """);

            migrationBuilder.DropTable(
                name: "user_login_state",
                schema: "identity");
        }
    }
}
