using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class Fase3OngStatusCompleta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EhOng",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Pets",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Interesses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Interesses_PetId",
                table: "Interesses",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Interesses_UsuarioInteressadoId",
                table: "Interesses",
                column: "UsuarioInteressadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interesses_Pets_PetId",
                table: "Interesses",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Interesses_Usuarios_UsuarioInteressadoId",
                table: "Interesses",
                column: "UsuarioInteressadoId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interesses_Pets_PetId",
                table: "Interesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Interesses_Usuarios_UsuarioInteressadoId",
                table: "Interesses");

            migrationBuilder.DropIndex(
                name: "IX_Interesses_PetId",
                table: "Interesses");

            migrationBuilder.DropIndex(
                name: "IX_Interesses_UsuarioInteressadoId",
                table: "Interesses");

            migrationBuilder.DropColumn(
                name: "EhOng",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Interesses");
        }
    }
}
