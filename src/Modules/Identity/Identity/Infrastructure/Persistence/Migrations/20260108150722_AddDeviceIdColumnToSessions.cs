using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceIdColumnToSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_deleted",
                schema: "identity",
                table: "sessions",
                newName: "is_revoked"
            );

            migrationBuilder.RenameColumn(
                name: "deleted_at",
                schema: "identity",
                table: "sessions",
                newName: "revoked_at"
            );

            migrationBuilder.RenameIndex(
                name: "ix_sessions_is_deleted",
                schema: "identity",
                table: "sessions",
                newName: "ix_sessions_is_revoked"
            );

            migrationBuilder.AddColumn<string>(
                name: "device_id",
                schema: "identity",
                table: "sessions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: ""
            );

            migrationBuilder.CreateIndex(
                name: "ix_sessions_device_id",
                schema: "identity",
                table: "sessions",
                column: "device_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_sessions_user_id_device_id",
                schema: "identity",
                table: "sessions",
                columns: new[] { "user_id", "device_id" },
                unique: true,
                filter: "[is_revoked] = false"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "ix_sessions_device_id", schema: "identity", table: "sessions");

            migrationBuilder.DropIndex(name: "ix_sessions_user_id_device_id", schema: "identity", table: "sessions");

            migrationBuilder.DropColumn(name: "device_id", schema: "identity", table: "sessions");

            migrationBuilder.RenameColumn(
                name: "revoked_at",
                schema: "identity",
                table: "sessions",
                newName: "deleted_at"
            );

            migrationBuilder.RenameColumn(
                name: "is_revoked",
                schema: "identity",
                table: "sessions",
                newName: "is_deleted"
            );

            migrationBuilder.RenameIndex(
                name: "ix_sessions_is_revoked",
                schema: "identity",
                table: "sessions",
                newName: "ix_sessions_is_deleted"
            );
        }
    }
}
