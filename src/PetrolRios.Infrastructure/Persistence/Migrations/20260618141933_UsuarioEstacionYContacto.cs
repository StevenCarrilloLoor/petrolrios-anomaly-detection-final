using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UsuarioEstacionYContacto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstacionId",
                table: "usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorreoContacto",
                table: "estaciones",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_EstacionId",
                table: "usuarios",
                column: "EstacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuarios_EstacionId",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "EstacionId",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "CorreoContacto",
                table: "estaciones");
        }
    }
}
