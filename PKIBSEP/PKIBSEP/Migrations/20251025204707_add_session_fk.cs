﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PKIBSEP.Migrations
{
    /// <inheritdoc />
    public partial class add_session_fk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_UserId",
                table: "Sessions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_UserId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions");
        }
    }
}
