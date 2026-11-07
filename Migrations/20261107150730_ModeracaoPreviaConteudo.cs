using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class ModeracaoPreviaConteudo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BioPendente",
                table: "Usuarios",
                type: "character varying(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescricaoOngPendente",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPerfilPendente",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Aprovado",
                table: "Postagens",
                type: "boolean",
                nullable: false,
                defaultValue: true); // tudo o que já existia continua aprovado

            migrationBuilder.AddColumn<bool>(
                name: "Aprovado",
                table: "Depoimentos",
                type: "boolean",
                nullable: false,
                defaultValue: true); // tudo o que já existia continua aprovado

            migrationBuilder.AddColumn<bool>(
                name: "Aprovado",
                table: "Comentarios",
                type: "boolean",
                nullable: false,
                defaultValue: true); // tudo o que já existia continua aprovado
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BioPendente",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DescricaoOngPendente",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FotoPerfilPendente",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Aprovado",
                table: "Postagens");

            migrationBuilder.DropColumn(
                name: "Aprovado",
                table: "Depoimentos");

            migrationBuilder.DropColumn(
                name: "Aprovado",
                table: "Comentarios");
        }
    }
}
