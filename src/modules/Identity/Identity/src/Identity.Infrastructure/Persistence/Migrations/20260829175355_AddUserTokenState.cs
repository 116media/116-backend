using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTokenState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_token_state",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    security_stamp = table.Column<Guid>(type: "uuid", nullable: false),
                    token_version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_token_state", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_user_token_state_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill: every existing user gets an invalidation record.
            migrationBuilder.Sql(
                """
                INSERT INTO identity.user_token_state (user_id, security_stamp, token_version, created_at)
                SELECT id, gen_random_uuid(), 0, now()
                FROM identity.users;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_token_state",
                schema: "identity");
        }
    }
}
