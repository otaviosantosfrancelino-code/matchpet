using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class RedeSocialChatTermosEstatisticas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Usuarios",
                type: "character varying(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdotanteId",
                table: "Pets",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAdocao",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCadastro",
                table: "Pets",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.CreateTable(
                name: "Amizades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SolicitanteId = table.Column<int>(type: "integer", nullable: false),
                    DestinatarioId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amizades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Amizades_Usuarios_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Amizades_Usuarios_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Conversas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IniciadorId = table.Column<int>(type: "integer", nullable: false),
                    DestinatarioId = table.Column<int>(type: "integer", nullable: false),
                    PetId = table.Column<int>(type: "integer", nullable: true),
                    ItemDoacaoId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimaMensagemEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversas_ItensDoacao_ItemDoacaoId",
                        column: x => x.ItemDoacaoId,
                        principalTable: "ItensDoacao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Conversas_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Conversas_Usuarios_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Conversas_Usuarios_IniciadorId",
                        column: x => x.IniciadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Depoimentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    PetId = table.Column<int>(type: "integer", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Texto = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    FotoUrl = table.Column<string>(type: "text", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depoimentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Depoimentos_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Depoimentos_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Midias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Dados = table.Column<byte[]>(type: "bytea", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    Tamanho = table.Column<long>(type: "bigint", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Midias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Midias_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Postagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    MidiaUrl = table.Column<string>(type: "text", nullable: true),
                    TipoMidia = table.Column<string>(type: "text", nullable: true),
                    PetId = table.Column<int>(type: "integer", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postagens_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Postagens_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TermosAdocao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PetId = table.Column<int>(type: "integer", nullable: false),
                    DoadorId = table.Column<int>(type: "integer", nullable: false),
                    AdotanteId = table.Column<int>(type: "integer", nullable: false),
                    ConversaId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Codigo = table.Column<string>(type: "text", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpDoador = table.Column<string>(type: "text", nullable: false),
                    DataAssinatura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IpAdotante = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermosAdocao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TermosAdocao_Conversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "Conversas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TermosAdocao_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TermosAdocao_Usuarios_AdotanteId",
                        column: x => x.AdotanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TermosAdocao_Usuarios_DoadorId",
                        column: x => x.DoadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Comentarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostagemId = table.Column<int>(type: "integer", nullable: false),
                    AutorId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comentarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comentarios_Postagens_PostagemId",
                        column: x => x.PostagemId,
                        principalTable: "Postagens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comentarios_Usuarios_AutorId",
                        column: x => x.AutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Curtidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostagemId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curtidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Curtidas_Postagens_PostagemId",
                        column: x => x.PostagemId,
                        principalTable: "Postagens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Curtidas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConversaId = table.Column<int>(type: "integer", nullable: false),
                    RemetenteId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    MidiaUrl = table.Column<string>(type: "text", nullable: true),
                    TermoAdocaoId = table.Column<int>(type: "integer", nullable: true),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_Conversas_ConversaId",
                        column: x => x.ConversaId,
                        principalTable: "Conversas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensagens_TermosAdocao_TermoAdocaoId",
                        column: x => x.TermoAdocaoId,
                        principalTable: "TermosAdocao",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Mensagens_Usuarios_RemetenteId",
                        column: x => x.RemetenteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pets_AdotanteId",
                table: "Pets",
                column: "AdotanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pets_Status",
                table: "Pets",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Amizades_DestinatarioId",
                table: "Amizades",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Amizades_SolicitanteId_DestinatarioId",
                table: "Amizades",
                columns: new[] { "SolicitanteId", "DestinatarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_AutorId",
                table: "Comentarios",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Comentarios_PostagemId",
                table: "Comentarios",
                column: "PostagemId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_DestinatarioId",
                table: "Conversas",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_IniciadorId",
                table: "Conversas",
                column: "IniciadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_ItemDoacaoId",
                table: "Conversas",
                column: "ItemDoacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversas_PetId",
                table: "Conversas",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_PostagemId_UsuarioId",
                table: "Curtidas",
                columns: new[] { "PostagemId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Curtidas_UsuarioId",
                table: "Curtidas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Depoimentos_AutorId",
                table: "Depoimentos",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Depoimentos_PetId",
                table: "Depoimentos",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_ConversaId_DataEnvio",
                table: "Mensagens",
                columns: new[] { "ConversaId", "DataEnvio" });

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_TermoAdocaoId",
                table: "Mensagens",
                column: "TermoAdocaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Midias_UsuarioId",
                table: "Midias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Postagens_AutorId",
                table: "Postagens",
                column: "AutorId");

            migrationBuilder.CreateIndex(
                name: "IX_Postagens_DataCriacao",
                table: "Postagens",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Postagens_PetId",
                table: "Postagens",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAdocao_AdotanteId",
                table: "TermosAdocao",
                column: "AdotanteId");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAdocao_ConversaId",
                table: "TermosAdocao",
                column: "ConversaId");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAdocao_DoadorId",
                table: "TermosAdocao",
                column: "DoadorId");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAdocao_PetId",
                table: "TermosAdocao",
                column: "PetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Usuarios_AdotanteId",
                table: "Pets",
                column: "AdotanteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Usuarios_AdotanteId",
                table: "Pets");

            migrationBuilder.DropTable(
                name: "Amizades");

            migrationBuilder.DropTable(
                name: "Comentarios");

            migrationBuilder.DropTable(
                name: "Curtidas");

            migrationBuilder.DropTable(
                name: "Depoimentos");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "Midias");

            migrationBuilder.DropTable(
                name: "Postagens");

            migrationBuilder.DropTable(
                name: "TermosAdocao");

            migrationBuilder.DropTable(
                name: "Conversas");

            migrationBuilder.DropIndex(
                name: "IX_Pets_AdotanteId",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_Pets_Status",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AdotanteId",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "DataAdocao",
                table: "Pets");

            migrationBuilder.DropColumn(
                name: "DataCadastro",
                table: "Pets");
        }
    }
}
