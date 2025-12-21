using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKIBSEP.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateKeyRefColumnOnCertificate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrivateKeyRef",
                table: "Certificate2s",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrivateKeyRef",
                table: "Certificate2s");
        }
    }
}
