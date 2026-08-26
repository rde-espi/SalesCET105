using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPromoCodeToMarcacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PercentagemDescontoAplicada",
                table: "Marcacoes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromoCodeId",
                table: "Marcacoes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorDesconto",
                table: "Marcacoes",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marcacoes_PromoCodeId",
                table: "Marcacoes",
                column: "PromoCodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Marcacoes_PromoCodes_PromoCodeId",
                table: "Marcacoes",
                column: "PromoCodeId",
                principalTable: "PromoCodes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Marcacoes_PromoCodes_PromoCodeId",
                table: "Marcacoes");

            migrationBuilder.DropIndex(
                name: "IX_Marcacoes_PromoCodeId",
                table: "Marcacoes");

            migrationBuilder.DropColumn(
                name: "PercentagemDescontoAplicada",
                table: "Marcacoes");

            migrationBuilder.DropColumn(
                name: "PromoCodeId",
                table: "Marcacoes");

            migrationBuilder.DropColumn(
                name: "ValorDesconto",
                table: "Marcacoes");
        }
    }
}
