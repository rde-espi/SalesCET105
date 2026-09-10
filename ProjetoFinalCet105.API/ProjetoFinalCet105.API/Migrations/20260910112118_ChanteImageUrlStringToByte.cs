using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class ChanteImageUrlStringToByte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagemUrl",
                table: "Categorias",
                newName: "ImagemContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "Imagem",
                table: "Categorias",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imagem",
                table: "Categorias");

            migrationBuilder.RenameColumn(
                name: "ImagemContentType",
                table: "Categorias",
                newName: "ImagemUrl");
        }
    }
}
