using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class AddImagemBinariaServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagemUrl",
                table: "Servicos",
                newName: "ImagemContentType");

            migrationBuilder.AddColumn<byte[]>(
                name: "Imagem",
                table: "Servicos",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imagem",
                table: "Servicos");

            migrationBuilder.RenameColumn(
                name: "ImagemContentType",
                table: "Servicos",
                newName: "ImagemUrl");
        }
    }
}