using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class AtualizaUsuarioJuridico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Interesses_Pets_PetId",
                table: "Interesses");

            migrationBuilder.DropForeignKey(
                name: "FK_Interesses_Usuarios_UsuarioAdotanteId",
                table: "Interesses");

            migrationBuilder.DropIndex(
                name: "IX_Interesses_PetId",
                table: "Interesses");

            migrationBuilder.DropIndex(
                name: "IX_Interesses_UsuarioAdotanteId",
                table: "Interesses");

            migrationBuilder.DropColumn(
                name: "CNPJ",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CPF",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RazaoSocial",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IpAceite",
                table: "Interesses");

            migrationBuilder.RenameColumn(
                name: "VersaoTermo",
                table: "Interesses",
                newName: "IpUsuario");

            migrationBuilder.RenameColumn(
                name: "UsuarioAdotanteId",
                table: "Interesses",
                newName: "UsuarioInteressadoId");

            migrationBuilder.RenameColumn(
                name: "DataAceite",
                table: "Interesses",
                newName: "DataInteresse");

            migrationBuilder.AlterColumn<string>(
                name: "IpAceite",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Documento",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Nome",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Documento",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Nome",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "UsuarioInteressadoId",
                table: "Interesses",
                newName: "UsuarioAdotanteId");

            migrationBuilder.RenameColumn(
                name: "IpUsuario",
                table: "Interesses",
                newName: "VersaoTermo");

            migrationBuilder.RenameColumn(
                name: "DataInteresse",
                table: "Interesses",
                newName: "DataAceite");

            migrationBuilder.AlterColumn<string>(
                name: "IpAceite",
                table: "Usuarios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CNPJ",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RazaoSocial",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IpAceite",
                table: "Interesses",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interesses_PetId",
                table: "Interesses",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Interesses_UsuarioAdotanteId",
                table: "Interesses",
                column: "UsuarioAdotanteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Interesses_Pets_PetId",
                table: "Interesses",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Interesses_Usuarios_UsuarioAdotanteId",
                table: "Interesses",
                column: "UsuarioAdotanteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
