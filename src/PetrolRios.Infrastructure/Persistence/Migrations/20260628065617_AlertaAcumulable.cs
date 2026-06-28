using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AlertaAcumulable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventosAcumulados",
                table: "alertas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "alertas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            // Las alertas EXISTENTES igualan FechaActualizacion a su FechaDeteccion, para no re-ordenarlas
            // todas arriba por la migración: solo las que se acumulen luego subirán (FechaActualizacion > FechaDeteccion).
            migrationBuilder.Sql("UPDATE \"alertas\" SET \"FechaActualizacion\" = \"FechaDeteccion\";");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_EstacionId_TransaccionReferencia",
                table: "alertas",
                columns: new[] { "EstacionId", "TransaccionReferencia" });

            migrationBuilder.CreateIndex(
                name: "IX_alertas_FechaActualizacion",
                table: "alertas",
                column: "FechaActualizacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_alertas_EstacionId_TransaccionReferencia",
                table: "alertas");

            migrationBuilder.DropIndex(
                name: "IX_alertas_FechaActualizacion",
                table: "alertas");

            migrationBuilder.DropColumn(
                name: "EventosAcumulados",
                table: "alertas");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "alertas");
        }
    }
}
