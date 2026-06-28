using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreciosCombustibleSistemaApiLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FuenteApi",
                table: "precios_combustible",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioApi",
                table: "precios_combustible",
                type: "numeric(8,4)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrecioApiActualizadoEn",
                table: "precios_combustible",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrecioPendiente",
                table: "precios_combustible",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "precios_combustible_log",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Producto = table.Column<int>(type: "integer", nullable: false),
                    PrecioAnterior = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    PrecioNuevo = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    VariacionPorcentual = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    Fuente = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Disparo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Resultado = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PrecioPendiente = table.Column<bool>(type: "boolean", nullable: false),
                    FuenteDegradada = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AdminId = table.Column<int>(type: "integer", nullable: true),
                    EtagRecibido = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RawHtmlHash = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    JitterSegundos = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precios_combustible_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_precios_combustible_log_Producto_CreatedAt",
                table: "precios_combustible_log",
                columns: new[] { "Producto", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "precios_combustible_log");

            migrationBuilder.DropColumn(
                name: "FuenteApi",
                table: "precios_combustible");

            migrationBuilder.DropColumn(
                name: "PrecioApi",
                table: "precios_combustible");

            migrationBuilder.DropColumn(
                name: "PrecioApiActualizadoEn",
                table: "precios_combustible");

            migrationBuilder.DropColumn(
                name: "PrecioPendiente",
                table: "precios_combustible");
        }
    }
}
