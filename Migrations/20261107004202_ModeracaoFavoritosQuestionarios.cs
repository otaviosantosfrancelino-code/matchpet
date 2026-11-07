using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class ModeracaoFavoritosQuestionarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Bloqueado",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EhAdmin",
                table: "Usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBloqueio",
                table: "Usuarios",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AvaliacoesUso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    Perfil = table.Column<string>(type: "text", nullable: false),
                    Respostas = table.Column<string>(type: "text", nullable: false),
                    Pontuacao = table.Column<double>(type: "double precision", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvaliacoesUso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvaliacoesUso_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    Xml = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Denuncias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DenuncianteId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "text", nullable: false),
                    AlvoId = table.Column<int>(type: "integer", nullable: false),
                    Motivo = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Detalhes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvidaPorId = table.Column<int>(type: "integer", nullable: true),
                    DataResolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resolucao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Denuncias_Usuarios_DenuncianteId",
                        column: x => x.DenuncianteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    PetId = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Favoritos_Pets_PetId",
                        column: x => x.PetId,
                        principalTable: "Pets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PerfisAdotante",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Moradia = table.Column<string>(type: "text", nullable: false),
                    MoradiaPropria = table.Column<bool>(type: "boolean", nullable: false),
                    ProprietarioPermite = table.Column<bool>(type: "boolean", nullable: false),
                    TemQuintal = table.Column<bool>(type: "boolean", nullable: false),
                    TelasDeProtecao = table.Column<bool>(type: "boolean", nullable: false),
                    TemCriancas = table.Column<bool>(type: "boolean", nullable: false),
                    TemOutrosAnimais = table.Column<bool>(type: "boolean", nullable: false),
                    OutrosAnimais = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HorasSozinho = table.Column<int>(type: "integer", nullable: false),
                    JaTevePets = table.Column<bool>(type: "boolean", nullable: false),
                    TodosConcordam = table.Column<bool>(type: "boolean", nullable: false),
                    CienteDosCustos = table.Column<bool>(type: "boolean", nullable: false),
                    AceitaAcompanhamento = table.Column<bool>(type: "boolean", nullable: false),
                    Motivacao = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: true),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PerfisAdotante", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PerfisAdotante_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvaliacoesUso_UsuarioId",
                table: "AvaliacoesUso",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_DenuncianteId",
                table: "Denuncias",
                column: "DenuncianteId");

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_Status_DataCriacao",
                table: "Denuncias",
                columns: new[] { "Status", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Denuncias_Tipo_AlvoId",
                table: "Denuncias",
                columns: new[] { "Tipo", "AlvoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_PetId",
                table: "Favoritos",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_UsuarioId_PetId",
                table: "Favoritos",
                columns: new[] { "UsuarioId", "PetId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerfisAdotante_UsuarioId",
                table: "PerfisAdotante",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvaliacoesUso");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "Denuncias");

            migrationBuilder.DropTable(
                name: "Favoritos");

            migrationBuilder.DropTable(
                name: "PerfisAdotante");

            migrationBuilder.DropColumn(
                name: "Bloqueado",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "EhAdmin",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoBloqueio",
                table: "Usuarios");
        }
    }
}
