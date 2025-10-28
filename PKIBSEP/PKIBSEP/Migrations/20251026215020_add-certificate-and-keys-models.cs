using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PKIBSEP.Migrations
{
    /// <inheritdoc />
    public partial class addcertificateandkeysmodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaUserId = table.Column<int>(type: "integer", nullable: false),
                    ChainRootCertificateId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Organization = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaAssignments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaKeyMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CertificateId = table.Column<int>(type: "integer", nullable: false),
                    PfxFilePath = table.Column<string>(type: "text", nullable: false),
                    EncryptedPfxPassword = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    KeystoreAlias = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaKeyMaterials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaUserKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaUserId = table.Column<int>(type: "integer", nullable: false),
                    ProtectedWrapKey = table.Column<string>(type: "text", nullable: false),
                    KeyVersion = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaUserKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Certificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SerialHex = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    SubjectDistinguishedName = table.Column<string>(type: "text", nullable: false),
                    IssuerDistinguishedName = table.Column<string>(type: "text", nullable: false),
                    NotBeforeUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NotAfterUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCertificateAuthority = table.Column<bool>(type: "boolean", nullable: false),
                    PathLenConstraint = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Organization = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PemCertificate = table.Column<string>(type: "text", nullable: false),
                    ChainPem = table.Column<string>(type: "text", nullable: false),
                    ParentCertificateId = table.Column<int>(type: "integer", nullable: true),
                    ChainRootId = table.Column<int>(type: "integer", nullable: false),
                    OwnerUserId = table.Column<int>(type: "integer", nullable: true),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevocationReason = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaAssignments_CaUserId_ChainRootCertificateId",
                table: "CaAssignments",
                columns: new[] { "CaUserId", "ChainRootCertificateId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaKeyMaterials_CertificateId",
                table: "CaKeyMaterials",
                column: "CertificateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaUserKeys_CaUserId_KeyVersion",
                table: "CaUserKeys",
                columns: new[] { "CaUserId", "KeyVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ChainRootId",
                table: "Certificates",
                column: "ChainRootId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_ParentCertificateId",
                table: "Certificates",
                column: "ParentCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_SerialHex",
                table: "Certificates",
                column: "SerialHex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificates_Type",
                table: "Certificates",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaAssignments");

            migrationBuilder.DropTable(
                name: "CaKeyMaterials");

            migrationBuilder.DropTable(
                name: "CaUserKeys");

            migrationBuilder.DropTable(
                name: "Certificates");
        }
    }
}
