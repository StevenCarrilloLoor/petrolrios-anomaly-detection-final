using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IdempotenciaStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HashContenido",
                table: "transacciones_staging",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // Filas previas no tienen hash de contenido (se desconoce el original).
            // Se rellenan con su Id (único) para no chocar con el índice único; las
            // nuevas inserciones usan el SHA-256 real de 64 caracteres hexadecimales.
            migrationBuilder.Sql(
                "UPDATE transacciones_staging SET \"HashContenido\" = CAST(\"Id\" AS text) " +
                "WHERE \"HashContenido\" = '' OR \"HashContenido\" IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_staging_EstacionId_HashContenido",
                table: "transacciones_staging",
                columns: new[] { "EstacionId", "HashContenido" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_transacciones_staging_EstacionId_HashContenido",
                table: "transacciones_staging");

            migrationBuilder.DropColumn(
                name: "HashContenido",
                table: "transacciones_staging");
        }
    }
}
