using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ejecuciones_job",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AlertasGeneradas = table.Column<int>(type: "integer", nullable: false),
                    EstacionesProcesadas = table.Column<int>(type: "integer", nullable: false),
                    EstacionesConError = table.Column<int>(type: "integer", nullable: false),
                    DuracionSegundos = table.Column<double>(type: "double precision", nullable: false),
                    ErrorDetalle = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ejecuciones_job", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "estaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Zona = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    HoraApertura = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraCierre = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reglas_deteccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoDetector = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ParametroNombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ValorUmbral = table.Column<double>(type: "double precision", nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reglas_deteccion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "transacciones_staging",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionId = table.Column<int>(type: "integer", nullable: false),
                    TipoTransaccion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataJson = table.Column<string>(type: "jsonb", nullable: false),
                    FechaOriginal = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Procesada = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transacciones_staging", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoDetector = table.Column<int>(type: "integer", nullable: false),
                    NivelRiesgo = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false),
                    FechaDeteccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmpleadoCodigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TransaccionReferencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MetadataJson = table.Column<string>(type: "jsonb", nullable: true),
                    EstacionId = table.Column<int>(type: "integer", nullable: false),
                    EjecucionJobId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alertas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alertas_ejecuciones_job_EjecucionJobId",
                        column: x => x.EjecucionJobId,
                        principalTable: "ejecuciones_job",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_alertas_estaciones_EstacionId",
                        column: x => x.EstacionId,
                        principalTable: "estaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "estacion_watermarks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EstacionId = table.Column<int>(type: "integer", nullable: false),
                    UltimaExtraccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estacion_watermarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_estacion_watermarks_estaciones_EstacionId",
                        column: x => x.EstacionId,
                        principalTable: "estaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_RolId",
                        column: x => x.RolId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asignaciones_alerta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlertaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FechaResolucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asignaciones_alerta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_asignaciones_alerta_alertas_AlertaId",
                        column: x => x.AlertaId,
                        principalTable: "alertas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_asignaciones_alerta_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "logs_auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Accion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Entidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntidadId = table.Column<int>(type: "integer", nullable: true),
                    DetalleJson = table.Column<string>(type: "jsonb", nullable: true),
                    DireccionIp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_auditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_logs_auditoria_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked = table.Column<bool>(type: "boolean", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alertas_EjecucionJobId",
                table: "alertas",
                column: "EjecucionJobId");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_EstacionId",
                table: "alertas",
                column: "EstacionId");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_EstacionId_FechaDeteccion",
                table: "alertas",
                columns: new[] { "EstacionId", "FechaDeteccion" });

            migrationBuilder.CreateIndex(
                name: "IX_alertas_Estado",
                table: "alertas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_FechaDeteccion",
                table: "alertas",
                column: "FechaDeteccion");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_NivelRiesgo",
                table: "alertas",
                column: "NivelRiesgo");

            migrationBuilder.CreateIndex(
                name: "IX_alertas_TipoDetector",
                table: "alertas",
                column: "TipoDetector");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_alerta_AlertaId",
                table: "asignaciones_alerta",
                column: "AlertaId");

            migrationBuilder.CreateIndex(
                name: "IX_asignaciones_alerta_UsuarioId",
                table: "asignaciones_alerta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ejecuciones_job_Estado",
                table: "ejecuciones_job",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_ejecuciones_job_FechaInicio",
                table: "ejecuciones_job",
                column: "FechaInicio");

            migrationBuilder.CreateIndex(
                name: "IX_estacion_watermarks_EstacionId",
                table: "estacion_watermarks",
                column: "EstacionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_estaciones_Codigo",
                table: "estaciones",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_logs_auditoria_CreatedAt",
                table: "logs_auditoria",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_logs_auditoria_UsuarioId",
                table: "logs_auditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_UsuarioId",
                table: "refresh_tokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_reglas_deteccion_TipoDetector",
                table: "reglas_deteccion",
                column: "TipoDetector");

            migrationBuilder.CreateIndex(
                name: "IX_reglas_deteccion_TipoDetector_ParametroNombre",
                table: "reglas_deteccion",
                columns: new[] { "TipoDetector", "ParametroNombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_Nombre",
                table: "roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_staging_EstacionId",
                table: "transacciones_staging",
                column: "EstacionId");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_staging_EstacionId_Procesada",
                table: "transacciones_staging",
                columns: new[] { "EstacionId", "Procesada" });

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_staging_Procesada",
                table: "transacciones_staging",
                column: "Procesada");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_RolId",
                table: "usuarios",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asignaciones_alerta");

            migrationBuilder.DropTable(
                name: "estacion_watermarks");

            migrationBuilder.DropTable(
                name: "logs_auditoria");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "reglas_deteccion");

            migrationBuilder.DropTable(
                name: "transacciones_staging");

            migrationBuilder.DropTable(
                name: "alertas");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "ejecuciones_job");

            migrationBuilder.DropTable(
                name: "estaciones");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
