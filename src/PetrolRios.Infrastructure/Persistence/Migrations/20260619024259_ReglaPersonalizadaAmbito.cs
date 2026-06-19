using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetrolRios.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReglaPersonalizadaAmbito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Ambito",
                table: "reglas_personalizadas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Auditoria");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ambito",
                table: "reglas_personalizadas");
        }
    }
}
