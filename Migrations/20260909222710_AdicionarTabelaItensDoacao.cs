using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTabelaItensDoacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContratoGerado",
                table: "Interesses",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ItensDoacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    EspecieDestino = table.Column<string>(type: "text", nullable: false),
                    Categoria = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: false),
                    UsuarioDoadorId = table.Column<int>(type: "integer", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensDoacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensDoacao_Usuarios_UsuarioDoadorId",
                        column: x => x.UsuarioDoadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItemImagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ItemDoacaoId = table.Column<int>(type: "integer", nullable: false),
                    Base64Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemImagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemImagens_ItensDoacao_ItemDoacaoId",
                        column: x => x.ItemDoacaoId,
                        principalTable: "ItensDoacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemImagens_ItemDoacaoId",
                table: "ItemImagens",
                column: "ItemDoacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensDoacao_UsuarioDoadorId",
                table: "ItensDoacao",
                column: "UsuarioDoadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemImagens");

            migrationBuilder.DropTable(
                name: "ItensDoacao");

            migrationBuilder.DropColumn(
                name: "ContratoGerado",
                table: "Interesses");
        }
    }
}
