using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgregarComentariosYFechaResolucion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaResolucion",
                table: "alertas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "comentarios_alerta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comentarios_alerta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_comentarios_alerta_alertas_AlertaId",
                        column: x => x.AlertaId,
                        principalTable: "alertas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_comentarios_alerta_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_comentarios_alerta_AlertaId",
                table: "comentarios_alerta",
                column: "AlertaId");

            migrationBuilder.CreateIndex(
                name: "IX_comentarios_alerta_UsuarioId",
                table: "comentarios_alerta",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "comentarios_alerta");

            migrationBuilder.DropColumn(
                name: "FechaResolucion",
                table: "alertas");
        }
    }
}
