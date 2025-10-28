using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKIBSEP.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerIdToCaKeyMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "CaKeyMaterials",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "CaKeyMaterials");
        }
    }
}
