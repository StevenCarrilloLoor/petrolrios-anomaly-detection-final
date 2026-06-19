using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFuenteDatosEstadoEstacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fuentes_datos_estados_estacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FuenteDatosId = table.Column<int>(type: "integer", nullable: false),
                    EstacionId = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TablaExiste = table.Column<bool>(type: "boolean", nullable: false),
                    ColumnaWatermarkValida = table.Column<bool>(type: "boolean", nullable: false),
                    FilasLeidas = table.Column<int>(type: "integer", nullable: false),
                    FilasEnviadas = table.Column<int>(type: "integer", nullable: false),
                    TotalFilasEnviadas = table.Column<long>(type: "bigint", nullable: false),
                    UltimoError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    VersionFuente = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoReporte = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimoExito = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fuentes_datos_estados_estacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_fuentes_datos_estados_estacion_estaciones_EstacionId",
                        column: x => x.EstacionId,
                        principalTable: "estaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fuentes_datos_estados_estacion_fuentes_datos_FuenteDatosId",
                        column: x => x.FuenteDatosId,
                        principalTable: "fuentes_datos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fuentes_datos_estados_estacion_EstacionId",
                table: "fuentes_datos_estados_estacion",
                column: "EstacionId");

            migrationBuilder.CreateIndex(
                name: "IX_fuentes_datos_estados_estacion_FuenteDatosId_EstacionId",
                table: "fuentes_datos_estados_estacion",
                columns: new[] { "FuenteDatosId", "EstacionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fuentes_datos_estados_estacion");
        }
    }
}
