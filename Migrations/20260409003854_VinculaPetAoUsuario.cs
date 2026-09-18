using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class VinculaPetAoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "UsuarioDoadorId",
                table: "Pets",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FotoUrl",
                table: "Pets",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Pets",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pets_UsuarioDoadorId",
                table: "Pets",
                column: "UsuarioDoadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Usuarios_UsuarioDoadorId",
                table: "Pets",
                column: "UsuarioDoadorId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Usuarios_UsuarioDoadorId",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_Pets_UsuarioDoadorId",
                table: "Pets");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioDoadorId",
                table: "Pets",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "FotoUrl",
                table: "Pets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Pets",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
