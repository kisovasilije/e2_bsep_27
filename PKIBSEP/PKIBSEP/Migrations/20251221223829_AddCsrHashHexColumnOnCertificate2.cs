using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKIBSEP.Migrations
{
    /// <inheritdoc />
    public partial class AddCsrHashHexColumnOnCertificate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Certificate2s_IssuerId",
                table: "Certificate2s");

            migrationBuilder.AddColumn<string>(
                name: "CsrHashHex",
                table: "Certificate2s",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificate2s_IssuerId_SerialNumberHex",
                table: "Certificate2s",
                columns: new[] { "IssuerId", "SerialNumberHex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Certificate2s_IssuerId_SerialNumberHex",
                table: "Certificate2s");

            migrationBuilder.DropColumn(
                name: "CsrHashHex",
                table: "Certificate2s");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate2s_IssuerId",
                table: "Certificate2s",
                column: "IssuerId");
        }
    }
}
