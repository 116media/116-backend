using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Mailer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMailerOutboxAndNewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "mailer");

            migrationBuilder.CreateTable(
                name: "newsletter_subscribers",
                schema: "mailer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    confirmation_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    unsubscribe_token = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    confirmed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    unsubscribed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_newsletter_subscribers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_emails",
                schema: "mailer",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_address = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    html_body = table.Column<string>(type: "text", nullable: false),
                    text_body = table.Column<string>(type: "text", nullable: false),
                    template = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_emails", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_newsletter_subscribers_confirmation_token",
                schema: "mailer",
                table: "newsletter_subscribers",
                column: "confirmation_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_newsletter_subscribers_email",
                schema: "mailer",
                table: "newsletter_subscribers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_newsletter_subscribers_unsubscribe_token",
                schema: "mailer",
                table: "newsletter_subscribers",
                column: "unsubscribe_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_emails_status_next_attempt_at",
                schema: "mailer",
                table: "outbox_emails",
                columns: new[] { "status", "next_attempt_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "newsletter_subscribers",
                schema: "mailer");

            migrationBuilder.DropTable(
                name: "outbox_emails",
                schema: "mailer");
        }
    }
}
