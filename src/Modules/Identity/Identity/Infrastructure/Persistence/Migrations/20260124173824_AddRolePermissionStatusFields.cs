using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRolePermissionStatusFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "identity",
                table: "roles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "identity",
                table: "roles",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "identity",
                table: "roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                schema: "identity",
                table: "permissions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "identity",
                table: "permissions",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "identity",
                table: "permissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_roles_is_active",
                schema: "identity",
                table: "roles",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_roles_is_deleted",
                schema: "identity",
                table: "roles",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_is_active",
                schema: "identity",
                table: "permissions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_permissions_is_deleted",
                schema: "identity",
                table: "permissions",
                column: "is_deleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_roles_is_active",
                schema: "identity",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "IX_roles_is_deleted",
                schema: "identity",
                table: "roles");

            migrationBuilder.DropIndex(
                name: "IX_permissions_is_active",
                schema: "identity",
                table: "permissions");

            migrationBuilder.DropIndex(
                name: "IX_permissions_is_deleted",
                schema: "identity",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "identity",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "identity",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "identity",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                schema: "identity",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "identity",
                table: "permissions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "identity",
                table: "permissions");
        }
    }
}
