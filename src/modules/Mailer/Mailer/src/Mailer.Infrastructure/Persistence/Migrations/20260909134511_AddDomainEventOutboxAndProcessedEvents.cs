using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Mailer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEventOutboxAndProcessedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "lease_expires_at",
                schema: "mailer",
                table: "outbox_emails",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "domain_event_outbox",
                schema: "mailer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    occurred_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    dispatched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_domain_event_outbox", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processed_domain_events",
                schema: "mailer",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    handler_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_domain_events", x => new { x.event_id, x.handler_name });
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_emails_lease_expires_at",
                schema: "mailer",
                table: "outbox_emails",
                column: "lease_expires_at");

            migrationBuilder.CreateIndex(
                name: "ix_domain_event_outbox_pending",
                schema: "mailer",
                table: "domain_event_outbox",
                column: "occurred_on",
                filter: "dispatched_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_event_outbox",
                schema: "mailer");

            migrationBuilder.DropTable(
                name: "processed_domain_events",
                schema: "mailer");

            migrationBuilder.DropIndex(
                name: "ix_outbox_emails_lease_expires_at",
                schema: "mailer",
                table: "outbox_emails");

            migrationBuilder.DropColumn(
                name: "lease_expires_at",
                schema: "mailer",
                table: "outbox_emails");
        }
    }
}
