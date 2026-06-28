using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PreciosCombustible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "precios_combustible",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Producto = table.Column<int>(type: "integer", nullable: false),
                    PrecioGalon = table.Column<decimal>(type: "numeric(8,4)", nullable: false),
                    Subsidio = table.Column<decimal>(type: "numeric(8,4)", nullable: false),
                    VigenteDesde = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VigenteHasta = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Fuente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_precios_combustible", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_precios_combustible_Producto",
                table: "precios_combustible",
                column: "Producto",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "precios_combustible");
        }
    }
}
