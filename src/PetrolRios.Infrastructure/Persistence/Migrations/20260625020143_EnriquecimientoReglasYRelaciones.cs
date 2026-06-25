using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EnriquecimientoReglasYRelaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CamposMostrarJson",
                table: "reglas_personalizadas",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "relaciones_tabla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FuenteOrigen = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FuenteDestino = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CampoOrigen = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CampoDestino = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Etiqueta = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relaciones_tabla", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_relaciones_tabla_FuenteOrigen_FuenteDestino_CampoOrigen_Cam~",
                table: "relaciones_tabla",
                columns: new[] { "FuenteOrigen", "FuenteDestino", "CampoOrigen", "CampoDestino" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "relaciones_tabla");

            migrationBuilder.DropColumn(
                name: "CamposMostrarJson",
                table: "reglas_personalizadas");
        }
    }
}
