using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissaoAdminTemporaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermissoesAdminTemporarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FuncionarioUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConcedidoPorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Revogada = table.Column<bool>(type: "bit", nullable: false),
                    DataRevogacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissoesAdminTemporarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermissoesAdminTemporarias_AspNetUsers_ConcedidoPorUserId",
                        column: x => x.ConcedidoPorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PermissoesAdminTemporarias_AspNetUsers_FuncionarioUserId",
                        column: x => x.FuncionarioUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesAdminTemporarias_ConcedidoPorUserId",
                table: "PermissoesAdminTemporarias",
                column: "ConcedidoPorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesAdminTemporarias_FuncionarioUserId_DataFim_Revogada",
                table: "PermissoesAdminTemporarias",
                columns: new[] { "FuncionarioUserId", "DataFim", "Revogada" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissoesAdminTemporarias");
        }
    }
}
