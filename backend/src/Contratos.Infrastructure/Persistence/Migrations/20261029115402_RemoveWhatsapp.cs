using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contratos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWhatsapp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusWhatsapp",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "WhatsappDestinatario",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "WhatsappEnviadoEm",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "WhatsappErroMensagem",
                table: "Contratos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StatusWhatsapp",
                table: "Contratos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatsappDestinatario",
                table: "Contratos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "WhatsappEnviadoEm",
                table: "Contratos",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WhatsappErroMensagem",
                table: "Contratos",
                type: "TEXT",
                nullable: true);
        }
    }
}
