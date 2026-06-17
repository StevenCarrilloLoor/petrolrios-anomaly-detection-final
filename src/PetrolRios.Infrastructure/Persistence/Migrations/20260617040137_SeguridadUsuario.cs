using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeguridadUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "usuarios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "DebeCambiarPassword",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEnd",
                table: "usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TotpHabilitado",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TotpSecret",
                table: "usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "DebeCambiarPassword",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "TotpHabilitado",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "TotpSecret",
                table: "usuarios");
        }
    }
}
