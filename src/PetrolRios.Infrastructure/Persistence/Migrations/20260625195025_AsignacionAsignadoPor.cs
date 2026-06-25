using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AsignacionAsignadoPor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AsignadoPorId",
                table: "asignaciones_alerta",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_alerta_AsignadoPorId",
                table: "asignaciones_alerta",
                column: "AsignadoPorId");

            migrationBuilder.AddForeignKey(
                name: "FK_asignaciones_alerta_usuarios_AsignadoPorId",
                table: "asignaciones_alerta",
                column: "AsignadoPorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_asignaciones_alerta_usuarios_AsignadoPorId",
                table: "asignaciones_alerta");

            migrationBuilder.DropIndex(
                name: "IX_asignaciones_alerta_AsignadoPorId",
                table: "asignaciones_alerta");

            migrationBuilder.DropColumn(
                name: "AsignadoPorId",
                table: "asignaciones_alerta");
        }
    }
}
