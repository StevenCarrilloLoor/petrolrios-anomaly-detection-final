using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHeartbeatYReglasPersonalizadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoHeartbeat",
                table: "estaciones",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionAgente",
                table: "estaciones",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reglas_personalizadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FuenteDatos = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CondicionesJson = table.Column<string>(type: "jsonb", nullable: false),
                    AgregacionJson = table.Column<string>(type: "jsonb", nullable: true),
                    RiesgoBase = table.Column<double>(type: "double precision", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reglas_personalizadas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reglas_personalizadas_Activa",
                table: "reglas_personalizadas",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_reglas_personalizadas_Nombre",
                table: "reglas_personalizadas",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reglas_personalizadas");

            migrationBuilder.DropColumn(
                name: "UltimoHeartbeat",
                table: "estaciones");

            migrationBuilder.DropColumn(
                name: "VersionAgente",
                table: "estaciones");
        }
    }
}
