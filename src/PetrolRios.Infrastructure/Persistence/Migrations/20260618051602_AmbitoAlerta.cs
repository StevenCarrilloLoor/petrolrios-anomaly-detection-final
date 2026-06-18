using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AmbitoAlerta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 2 = AmbitoAlerta.Auditoria. Las alertas previas se clasifican como auditoría
            // (carril del central) para no romper el comportamiento existente.
            migrationBuilder.AddColumn<int>(
                name: "Ambito",
                table: "alertas",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_alertas_Ambito",
                table: "alertas",
                column: "Ambito");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_Ambito_EstacionId_FechaDeteccion",
                table: "alertas",
                columns: new[] { "Ambito", "EstacionId", "FechaDeteccion" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_alertas_Ambito",
                table: "alertas");

            migrationBuilder.DropIndex(
                name: "IX_alertas_Ambito_EstacionId_FechaDeteccion",
                table: "alertas");

            migrationBuilder.DropColumn(
                name: "Ambito",
                table: "alertas");
        }
    }
}
