using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaPerfilEDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Classificacao",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimaAlteracao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoDocumentoBase64",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Classificacao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataUltimaAlteracao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FotoDocumentoBase64",
                table: "Usuarios");
        }
    }
}
