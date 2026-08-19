using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _116.Identity.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class HashOtpCodeAtRest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Otps_UserId_Code_Purpose", schema: "identity", table: "otps");

            // OTP rows live for sixty minutes, so none of the existing ones outlive the next
            // deploy window. Clearing them keeps the cleartext codes from surviving the rename;
            // anybody mid-flow recovers through resend.
            migrationBuilder.Sql("DELETE FROM identity.otps;");

            migrationBuilder.RenameColumn(name: "code", schema: "identity", table: "otps", newName: "code_hash");

            migrationBuilder.AlterColumn<string>(
                name: "code_hash",
                schema: "identity",
                table: "otps",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(6)",
                oldMaxLength: 6
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Hashes do not fit the narrower column, and reversing them is impossible, so the
            // rows go the same way they did on the way up.
            migrationBuilder.Sql("DELETE FROM identity.otps;");

            migrationBuilder.AlterColumn<string>(
                name: "code_hash",
                schema: "identity",
                table: "otps",
                type: "character varying(6)",
                maxLength: 6,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100
            );

            migrationBuilder.RenameColumn(name: "code_hash", schema: "identity", table: "otps", newName: "code");

            migrationBuilder.CreateIndex(
                name: "IX_Otps_UserId_Code_Purpose",
                schema: "identity",
                table: "otps",
                columns: new[] { "user_id", "code", "purpose" }
            );
        }
    }
}
