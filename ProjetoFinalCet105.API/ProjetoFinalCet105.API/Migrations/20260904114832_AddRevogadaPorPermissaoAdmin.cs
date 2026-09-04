using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoFinalCet105.API.Migrations
{
    /// <inheritdoc />
    public partial class AddRevogadaPorPermissaoAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RevogadaPorUserId",
                table: "PermissoesAdminTemporarias",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesAdminTemporarias_RevogadaPorUserId",
                table: "PermissoesAdminTemporarias",
                column: "RevogadaPorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PermissoesAdminTemporarias_AspNetUsers_RevogadaPorUserId",
                table: "PermissoesAdminTemporarias",
                column: "RevogadaPorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PermissoesAdminTemporarias_AspNetUsers_RevogadaPorUserId",
                table: "PermissoesAdminTemporarias");

            migrationBuilder.DropIndex(
                name: "IX_PermissoesAdminTemporarias_RevogadaPorUserId",
                table: "PermissoesAdminTemporarias");

            migrationBuilder.DropColumn(
                name: "RevogadaPorUserId",
                table: "PermissoesAdminTemporarias");
        }
    }
}
