using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class AchadosEPerdidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnimalPerdidoId",
                table: "Depoimentos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnimalPerdidoId",
                table: "Conversas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AnimaisPerdidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    Nome = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Especie = table.Column<string>(type: "text", nullable: false),
                    Raca = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Porte = table.Column<string>(type: "text", nullable: false),
                    Sexo = table.Column<string>(type: "text", nullable: false),
                    Cor = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    UsaColeira = table.Column<bool>(type: "boolean", nullable: false),
                    SinaisParticulares = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Bairro = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    DataOcorrido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataResolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimoLembrete = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimaisPerdidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimaisPerdidos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnimaisPerdidosImagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AnimalPerdidoId = table.Column<int>(type: "integer", nullable: false),
                    Base64Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimaisPerdidosImagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnimaisPerdidosImagens_AnimaisPerdidos_AnimalPerdidoId",
                        column: x => x.AnimalPerdidoId,
                        principalTable: "AnimaisPerdidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Depoimentos_AnimalPerdidoId",
                table: "Depoimentos",
                column: "AnimalPerdidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_AnimalPerdidoId",
                table: "Conversas",
                column: "AnimalPerdidoId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimaisPerdidos_Status_Tipo",
                table: "AnimaisPerdidos",
                columns: new[] { "Status", "Tipo" });

            migrationBuilder.CreateIndex(
                name: "IX_AnimaisPerdidos_UsuarioId",
                table: "AnimaisPerdidos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AnimaisPerdidosImagens_AnimalPerdidoId",
                table: "AnimaisPerdidosImagens",
                column: "AnimalPerdidoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversas_AnimaisPerdidos_AnimalPerdidoId",
                table: "Conversas",
                column: "AnimalPerdidoId",
                principalTable: "AnimaisPerdidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Depoimentos_AnimaisPerdidos_AnimalPerdidoId",
                table: "Depoimentos",
                column: "AnimalPerdidoId",
                principalTable: "AnimaisPerdidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversas_AnimaisPerdidos_AnimalPerdidoId",
                table: "Conversas");

            migrationBuilder.DropForeignKey(
                name: "FK_Depoimentos_AnimaisPerdidos_AnimalPerdidoId",
                table: "Depoimentos");

            migrationBuilder.DropTable(
                name: "AnimaisPerdidosImagens");

            migrationBuilder.DropTable(
                name: "AnimaisPerdidos");

            migrationBuilder.DropIndex(
                name: "IX_Depoimentos_AnimalPerdidoId",
                table: "Depoimentos");

            migrationBuilder.DropIndex(
                name: "IX_Conversas_AnimalPerdidoId",
                table: "Conversas");

            migrationBuilder.DropColumn(
                name: "AnimalPerdidoId",
                table: "Depoimentos");

            migrationBuilder.DropColumn(
                name: "AnimalPerdidoId",
                table: "Conversas");
        }
    }
}
