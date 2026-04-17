using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profisys_Programming_Task.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeDeleteToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentItems_Documents_DocumentId",
                table: "DocumentItems");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentItems_Documents_DocumentId",
                table: "DocumentItems",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentItems_Documents_DocumentId",
                table: "DocumentItems");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentItems_Documents_DocumentId",
                table: "DocumentItems",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
