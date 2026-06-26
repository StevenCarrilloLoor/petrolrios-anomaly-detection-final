using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProgramacionDeRegla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProgramacionJson",
                table: "reglas_personalizadas",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaEjecucion",
                table: "reglas_personalizadas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaEjecucion",
                table: "reglas_personalizadas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProgramacionJson",
                table: "reglas_deteccion",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaEjecucion",
                table: "reglas_deteccion",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaEjecucion",
                table: "reglas_deteccion",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgramacionJson",
                table: "reglas_personalizadas");

            migrationBuilder.DropColumn(
                name: "ProximaEjecucion",
                table: "reglas_personalizadas");

            migrationBuilder.DropColumn(
                name: "UltimaEjecucion",
                table: "reglas_personalizadas");

            migrationBuilder.DropColumn(
                name: "ProgramacionJson",
                table: "reglas_deteccion");

            migrationBuilder.DropColumn(
                name: "ProximaEjecucion",
                table: "reglas_deteccion");

            migrationBuilder.DropColumn(
                name: "UltimaEjecucion",
                table: "reglas_deteccion");
        }
    }
}
