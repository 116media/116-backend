using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Mailer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DropRedundantProcessedDomainEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_domain_events",
                schema: "mailer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
