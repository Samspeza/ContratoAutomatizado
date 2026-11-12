using Contratos.Application.Messages;
using Contratos.Domain.Entities;
using Xunit;

namespace Contratos.Tests.Application;

public class PlaceholdersMensagemTests
{
    private static Contrato CriarContrato() =>
        Contrato.CriarRascunho(
            "12345678000190", "EMPRESA TESTE LTDA", "Rua Teste, 100",
            25, 3, new DateOnly(2026, 9, 30),
            new List<ContratoResponsavel> { new() { Nome = "Maria da Silva" } });

    [Fact]
    public void Substituir_ComPlaceholdersConhecidos_DeveSubstituirTodos()
    {
        var valores = PlaceholdersMensagem.Montar(CriarContrato());

        var resultado = PlaceholdersMensagem.Substituir(
            "Olá, {{NOME_RESPONSAVEL}}. Contrato de {{RAZAO_SOCIAL}} ({{CNPJ}}) em {{DATA_CONTRATO}}.", valores);

        Assert.Equal(
            "Olá, Maria da Silva. Contrato de EMPRESA TESTE LTDA (12.345.678/0001-90) em 30/09/2026.",
            resultado);
    }

    [Fact]
    public void Substituir_SemPlaceholders_DeveManterOTextoIdentico()
    {
        var valores = PlaceholdersMensagem.Montar(CriarContrato());
        const string texto = "Prezado(a),\nTexto sem campos.";

        Assert.Equal(texto, PlaceholdersMensagem.Substituir(texto, valores));
    }

    [Fact]
    public void Substituir_ComPlaceholderDesconhecido_DeveLancarExcecao()
    {
        var valores = PlaceholdersMensagem.Montar(CriarContrato());

        var excecao = Assert.Throws<InvalidOperationException>(
            () => PlaceholdersMensagem.Substituir("Olá, {{NOME}}.", valores));

        Assert.Contains("{{NOME}}", excecao.Message);
    }
    [Theory]
    [InlineData("{{nome_responsavel}}")]
    [InlineData("{{ CNPJ }}")]
    [InlineData("{{NOME}}")]
    public void NomesDesconhecidos_ComCampoInvalido_DeveListarOCampo(string texto)
    {
        Assert.Single(PlaceholdersMensagem.NomesDesconhecidos(texto));
    }

    [Fact]
    public void NomesDesconhecidos_ComCamposConhecidos_DeveRetornarVazio()
    {
        var texto = "Olá {{NOME_RESPONSAVEL}}, {{RAZAO_SOCIAL}} ({{CNPJ}}) em {{DATA_CONTRATO}}.";

        Assert.Empty(PlaceholdersMensagem.NomesDesconhecidos(texto));
    }

    [Fact]
    public void Substituir_ComCampoEmMinusculas_DeveLancarExcecaoEmVezDeEnviarLiteral()
    {
        var valores = PlaceholdersMensagem.Montar(CriarContrato());

        Assert.Throws<InvalidOperationException>(
            () => PlaceholdersMensagem.Substituir("Olá, {{nome_responsavel}}.", valores));
    }
}