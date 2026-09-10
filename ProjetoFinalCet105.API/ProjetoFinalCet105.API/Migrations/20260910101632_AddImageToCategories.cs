using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Categorias",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Categorias");
        }
    }
}