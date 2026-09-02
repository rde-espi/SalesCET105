using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class AddFaturacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Serie = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NumeroSequencial = table.Column<int>(type: "int", nullable: false),
                    MarcacaoId = table.Column<int>(type: "int", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DataEmissao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NomeCliente = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NifCliente = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ValorIva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ComunicadaAT = table.Column<bool>(type: "bit", nullable: false),
                    DataComunicacaoAT = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CodigoRespostaAT = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MensagemRespostaAT = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Faturas_Marcacoes_MarcacaoId",
                        column: x => x.MarcacaoId,
                        principalTable: "Marcacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FaturaItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaId = table.Column<int>(type: "int", nullable: false),
                    CodigoIva = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MotivoIsencaoIva = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ServicoId = table.Column<int>(type: "int", nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PercentagemIva = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ValorIva = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturaItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FaturaItens_Faturas_FaturaId",
                        column: x => x.FaturaId,
                        principalTable: "Faturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FaturaItens_FaturaId",
                table: "FaturaItens",
                column: "FaturaId");

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_MarcacaoId",
                table: "Faturas",
                column: "MarcacaoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_Numero",
                table: "Faturas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faturas_Serie_NumeroSequencial",
                table: "Faturas",
                columns: new[] { "Serie", "NumeroSequencial" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaturaItens");

            migrationBuilder.DropTable(
                name: "Faturas");
        }
    }
}
