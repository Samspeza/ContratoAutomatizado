using Contratos.Domain.Entities;
using Contratos.Domain.Enums;
using Xunit;

namespace Contratos.Tests.Domain;

public class ContratoTests
{
    private static Contrato CriarContratoGerado()
    {
        var contrato = Contrato.CriarRascunho(
            "12345678000190", "EMPRESA TESTE LTDA", "Rua Teste, 100",
            25, 3, new DateOnly(2026, 10, 28),
            new List<ContratoResponsavel> { new() { Nome = "Teste" } });

        contrato.MarcarComoGerado("caminho.docx", "caminho.pdf");
        return contrato;
    }

    [Fact]
    public void AvancarParaEnvio_DeGerado_DeveFicarProntoParaEnvioERegistrarEvento()
    {
        var contrato = CriarContratoGerado();

        contrato.AvancarParaEnvio();

        Assert.Equal(StatusContrato.ProntoParaEnvio, contrato.Status);
        Assert.Contains(contrato.Historico, e => e.Tipo == TipoEventoHistorico.AvancouParaEnvio);
    }

    [Fact]
    public void AvancarParaEnvio_ChamadoDuasVezes_NaoDeveDuplicarEvento()
    {
        var contrato = CriarContratoGerado();

        contrato.AvancarParaEnvio();
        contrato.AvancarParaEnvio();

        Assert.Single(contrato.Historico, e => e.Tipo == TipoEventoHistorico.AvancouParaEnvio);
    }

    [Fact]
    public void AvancarParaEnvio_ComContratoAindaNaoGerado_DeveLancarExcecao()
    {
        var contrato = Contrato.CriarRascunho(
            "12345678000190", "EMPRESA TESTE LTDA", "Rua Teste, 100",
            25, 3, new DateOnly(2026, 10, 28), new List<ContratoResponsavel>());

        Assert.Throws<InvalidOperationException>(() => contrato.AvancarParaEnvio());
    }

    [Fact]
    public void MarcarEmailEnviado_DeveFicarConcluidoELimparErroAnterior()
    {
        var contrato = CriarContratoGerado();
        contrato.AvancarParaEnvio();
        contrato.DefinirEmailDestinatario("cliente@empresa.com.br");
        contrato.MarcarErroEnvioEmail("falha");

        contrato.MarcarEmailEnviado();

        Assert.Equal(StatusContrato.Concluido, contrato.Status);
        Assert.Equal(StatusCanalEnvio.Enviado, contrato.StatusEmail);
        Assert.Null(contrato.EmailErroMensagem);
        Assert.NotNull(contrato.EmailEnviadoEm);
    }

    [Fact]
    public void MarcarErroEnvioEmail_DeveFicarErroNoEnvioERegistrarMotivo()
    {
        var contrato = CriarContratoGerado();
        contrato.AvancarParaEnvio();

        contrato.MarcarErroEnvioEmail("Não foi possível enviar o e-mail.");

        Assert.Equal(StatusContrato.ErroNoEnvio, contrato.Status);
        Assert.Equal(StatusCanalEnvio.Erro, contrato.StatusEmail);
        Assert.Equal("Não foi possível enviar o e-mail.", contrato.EmailErroMensagem);
        Assert.Contains(contrato.Historico, e => e.Tipo == TipoEventoHistorico.FalhaEnvioEmail);
    }

    [Fact]
    public void MarcarErroEnvioEmail_AposEnvioComSucesso_DeveManterADataDoUltimoEnvio()
    {
        var contrato = CriarContratoGerado();
        contrato.AvancarParaEnvio();
        contrato.DefinirEmailDestinatario("cliente@empresa.com.br");
        contrato.MarcarEmailEnviado();

        contrato.MarcarErroEnvioEmail("falha no reenvio");

        Assert.Equal(StatusContrato.ErroNoEnvio, contrato.Status);
        Assert.NotNull(contrato.EmailEnviadoEm);
    }

    [Theory]
    [InlineData(StatusContrato.Rascunho, false)]
    [InlineData(StatusContrato.ErroGeracao, false)]
    [InlineData(StatusContrato.Gerado, false)]
    [InlineData(StatusContrato.ProntoParaEnvio, true)]
    [InlineData(StatusContrato.ErroNoEnvio, true)]
    [InlineData(StatusContrato.Concluido, true)]
    [InlineData(StatusContrato.Cancelado, false)]
    public void PodeEnviarEmail_DependeDoStatus(StatusContrato status, bool esperado)
    {
        var contrato = CriarContratoGerado();
        contrato.Status = status;

        Assert.Equal(esperado, contrato.PodeEnviarEmail);
    }

    [Theory]
    [InlineData(StatusContrato.Rascunho, true)]
    [InlineData(StatusContrato.ErroGeracao, true)]
    [InlineData(StatusContrato.Gerado, false)]
    [InlineData(StatusContrato.Concluido, false)]
    public void PodeGerarArquivos_SoParaRascunhoOuErroDeGeracao(StatusContrato status, bool esperado)
    {
        var contrato = CriarContratoGerado();
        contrato.Status = status;

        Assert.Equal(esperado, contrato.PodeGerarArquivos);
    }

    [Fact]
    public void Cancelar_DeContratoProntoParaEnvio_DeveFicarCanceladoERegistrarEvento()
    {
        var contrato = CriarContratoGerado();
        contrato.AvancarParaEnvio();

        contrato.Cancelar();

        Assert.Equal(StatusContrato.Cancelado, contrato.Status);
        Assert.Contains(contrato.Historico, e => e.Tipo == TipoEventoHistorico.ContratoCancelado);
        Assert.False(contrato.PodeEnviarEmail);
    }

    [Theory]
    [InlineData(StatusContrato.Concluido)]
    [InlineData(StatusContrato.Cancelado)]
    public void Cancelar_DeContratoConcluidoOuJaCancelado_DeveLancarExcecao(StatusContrato status)
    {
        var contrato = CriarContratoGerado();
        contrato.Status = status;

        Assert.Throws<InvalidOperationException>(() => contrato.Cancelar());
    }
}