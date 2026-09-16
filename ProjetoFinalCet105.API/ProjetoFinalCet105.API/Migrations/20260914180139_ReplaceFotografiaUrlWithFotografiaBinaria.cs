using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceFotografiaUrlWithFotografiaBinaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FotografiaUrl",
                table: "AspNetUsers",
                newName: "FotografiaContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "Fotografia",
                table: "AspNetUsers",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fotografia",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "FotografiaContentType",
                table: "AspNetUsers",
                newName: "FotografiaUrl");
        }
    }
}