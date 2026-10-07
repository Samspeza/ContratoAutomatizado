using Contratos.Domain.Entities;
using Contratos.Domain.Enums;
using Xunit;

namespace Contratos.Tests.Domain;

public class ContratoTests
{
    private static Contrato CriarContratoProntoParaEnvio(string? email, string? whatsapp)
    {
        var contrato = Contrato.CriarRascunho(
            "12345678000190", "EMPRESA TESTE LTDA", "Rua Teste, 100",
            25, 3, new DateOnly(2026, 9, 29),
            new List<ContratoResponsavel> { new() { Nome = "Teste" } });

        contrato.MarcarComoGerado("caminho.docx", "caminho.pdf");
        contrato.EmailDestinatario = email;
        contrato.WhatsappDestinatario = whatsapp;
        contrato.Status = StatusContrato.ProntoParaEnvio;
        return contrato;
    }

    [Fact]
    public void RecalcularStatusDeEnvio_ComAmbosEnviados_DeveFicarConcluido()
    {
        var contrato = CriarContratoProntoParaEnvio("a@b.com", "+5517999999999");
        contrato.StatusEmail = StatusCanalEnvio.Enviado;
        contrato.StatusWhatsapp = StatusCanalEnvio.Enviado;

        contrato.RecalcularStatusDeEnvio();

        Assert.Equal(StatusContrato.Concluido, contrato.Status);
    }

    [Fact]
    public void RecalcularStatusDeEnvio_ComEmailEnviadoEWhatsappComErro_DeveFicarParcial()
    {
        var contrato = CriarContratoProntoParaEnvio("a@b.com", "+5517999999999");
        contrato.StatusEmail = StatusCanalEnvio.Enviado;
        contrato.StatusWhatsapp = StatusCanalEnvio.Erro;

        contrato.RecalcularStatusDeEnvio();

        Assert.Equal(StatusContrato.EnvioParcial, contrato.Status);
    }

    [Fact]
    public void RecalcularStatusDeEnvio_SoComEmailConfigurado_IgnoraWhatsapp()
    {
        var contrato = CriarContratoProntoParaEnvio("a@b.com", null);
        contrato.StatusEmail = StatusCanalEnvio.Enviado;
        // WhatsApp nunca foi configurado para este contrato — não deve impedir "Concluído".

        contrato.RecalcularStatusDeEnvio();

        Assert.Equal(StatusContrato.Concluido, contrato.Status);
    }

    [Fact]
    public void RecalcularStatusDeEnvio_ComAmbosComErro_DeveFicarErroNoEnvio()
    {
        var contrato = CriarContratoProntoParaEnvio("a@b.com", "+5517999999999");
        contrato.StatusEmail = StatusCanalEnvio.Erro;
        contrato.StatusWhatsapp = StatusCanalEnvio.Erro;

        contrato.RecalcularStatusDeEnvio();

        Assert.Equal(StatusContrato.ErroNoEnvio, contrato.Status);
    }

    [Fact]
    public void RecalcularStatusDeEnvio_QuandoAindaNaoGerado_NaoAltera()
    {
        var contrato = Contrato.CriarRascunho(
            "12345678000190", "EMPRESA TESTE LTDA", "Rua Teste, 100",
            25, 3, new DateOnly(2026, 9, 29), new List<ContratoResponsavel>());

        contrato.RecalcularStatusDeEnvio();

        Assert.Equal(StatusContrato.Rascunho, contrato.Status);
    }
}