using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace crudcomdb.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposOng : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChavePix",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescricaoOng",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChavePix",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DescricaoOng",
                table: "Usuarios");
        }
    }
}
