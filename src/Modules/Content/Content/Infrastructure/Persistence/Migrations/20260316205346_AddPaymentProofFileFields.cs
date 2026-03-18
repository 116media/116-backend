using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Content.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentProofFileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_proof_url",
                schema: "content",
                table: "content_payments");

            migrationBuilder.AddColumn<Guid>(
                name: "payment_proof_file_id",
                schema: "content",
                table: "content_payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_proof_mime_type",
                schema: "content",
                table: "content_payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_proof_storage_url",
                schema: "content",
                table: "content_payments",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_proof_file_id",
                schema: "content",
                table: "content_payments");

            migrationBuilder.DropColumn(
                name: "payment_proof_mime_type",
                schema: "content",
                table: "content_payments");

            migrationBuilder.DropColumn(
                name: "payment_proof_storage_url",
                schema: "content",
                table: "content_payments");

            migrationBuilder.AddColumn<string>(
                name: "payment_proof_url",
                schema: "content",
                table: "content_payments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
