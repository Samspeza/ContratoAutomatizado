using Contratos.Application.DTOs;
using Contratos.Infrastructure.Documents;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Contratos.Tests;

public class OpenXmlGeradorDocumentoServiceTests
{
    [Fact]
    public void GerarContrato_DeveSubstituirTodosOsPlaceholders()
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Documentos:CaminhoTemplate"] = "../../../../../../templates/contratos/modelo.docx"
            })
            .Build();

        var servico = new OpenXmlGeradorDocumentoService(configuracao);

        var dados = new DadosContratoDto(
            Empresa: new EmpresaDto("12345678000190", "EMPRESA TESTE LTDA", null, "Rua Teste", "100", null, "Centro", "São Paulo", "SP", "01000-000", null),
            ComplementoEndereco: null,
            PercentualHonorarios: 20,
            QuantidadeParcelas: 3,
            DataContrato: new DateOnly(2026, 9, 25),
            Responsaveis: new List<ResponsavelAssinaturaDto> { new("Nome Teste", "123.456.789-00") });

        var resultado = servico.GerarContrato(dados);

        Assert.NotEmpty(resultado);

        var caminhoSaida = Path.Combine(Path.GetTempPath(), "teste_contrato.docx");
        File.WriteAllBytes(caminhoSaida, resultado);
        Assert.True(File.Exists(caminhoSaida));
    }
}