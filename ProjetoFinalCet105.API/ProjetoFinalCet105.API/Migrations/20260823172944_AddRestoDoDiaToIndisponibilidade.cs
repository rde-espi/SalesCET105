using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRestoDoDiaToIndisponibilidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RestoDoDia",
                table: "Indisponibilidades",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestoDoDia",
                table: "Indisponibilidades");
        }
    }
}
