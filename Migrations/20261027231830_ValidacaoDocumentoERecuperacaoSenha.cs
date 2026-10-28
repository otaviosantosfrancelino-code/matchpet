using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class ValidacaoDocumentoERecuperacaoSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentoHash",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoObservacao",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetSenhaExpiraEm",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResetSenhaTokenHash",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentoHash",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DocumentoObservacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ResetSenhaExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "ResetSenhaTokenHash",
                table: "Usuarios");
        }
    }
}
