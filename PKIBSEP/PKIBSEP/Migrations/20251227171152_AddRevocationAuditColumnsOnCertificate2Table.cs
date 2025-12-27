using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKIBSEP.Migrations
{
    /// <inheritdoc />
    public partial class AddRevocationAuditColumnsOnCertificate2Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "Certificate2s",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RevocationReason",
                table: "Certificate2s",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                table: "Certificate2s",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevokedBy",
                table: "Certificate2s",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "Certificate2s");

            migrationBuilder.DropColumn(
                name: "RevocationReason",
                table: "Certificate2s");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                table: "Certificate2s");

            migrationBuilder.DropColumn(
                name: "RevokedBy",
                table: "Certificate2s");
        }
    }
}
