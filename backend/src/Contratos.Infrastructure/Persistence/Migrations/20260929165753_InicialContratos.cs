using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contratos.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialContratos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    RazaoSocial = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    PercentualHonorarios = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    QuantidadeParcelas = table.Column<int>(type: "INTEGER", nullable: false),
                    DataContrato = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    CaminhoArquivoDocx = table.Column<string>(type: "TEXT", nullable: true),
                    CaminhoArquivoPdf = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    EmailDestinatario = table.Column<string>(type: "TEXT", nullable: true),
                    StatusEmail = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    EmailEnviadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EmailErroMensagem = table.Column<string>(type: "TEXT", nullable: true),
                    WhatsappDestinatario = table.Column<string>(type: "TEXT", nullable: true),
                    StatusWhatsapp = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    WhatsappEnviadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WhatsappErroMensagem = table.Column<string>(type: "TEXT", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContratoResponsaveis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContratoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 14, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratoResponsaveis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratoResponsaveis_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventosHistorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContratoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 40, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosHistorico_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContratoResponsaveis_ContratoId",
                table: "ContratoResponsaveis",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Cnpj",
                table: "Contratos",
                column: "Cnpj");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_Status",
                table: "Contratos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EventosHistorico_ContratoId",
                table: "EventosHistorico",
                column: "ContratoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContratoResponsaveis");

            migrationBuilder.DropTable(
                name: "EventosHistorico");

            migrationBuilder.DropTable(
                name: "Contratos");
        }
    }
}
