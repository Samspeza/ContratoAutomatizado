using Contratos.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class ArmazenamentoContratoLocalServiceTests : IDisposable
{
    private readonly string _pastaTemporaria;
    private readonly ArmazenamentoContratoLocalService _servico;

    public ArmazenamentoContratoLocalServiceTests()
    {
        _pastaTemporaria = Path.Combine(Path.GetTempPath(), "ContratoAutomatizado_Testes_" + Guid.NewGuid());

        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Armazenamento:PastaContratosGerados"] = _pastaTemporaria
            })
            .Build();

        _servico = new ArmazenamentoContratoLocalService(configuracao);
    }

    [Fact]
    public void DeterminarNomeBaseDisponivel_SemArquivoExistente_DeveRetornarNomeSimples()
    {
        var nomeBase = _servico.DeterminarNomeBaseDisponivel("00000000000191");
        Assert.Equal("Contrato_00000000000191", nomeBase);
    }

    [Fact]
    public void DeterminarNomeBaseDisponivel_ComDocxExistente_DeveRetornarProximaVersao()
    {
        _servico.SalvarArquivo(new byte[] { 1, 2, 3 }, "Contrato_00000000000191", ".docx");

        var nomeBase = _servico.DeterminarNomeBaseDisponivel("00000000000191");

        Assert.Equal("Contrato_00000000000191_v2", nomeBase);
    }

    [Fact]
    public void SalvarArquivo_DeveGravarConteudoNoCaminhoEsperado()
    {
        var conteudo = new byte[] { 10, 20, 30 };
        var caminho = _servico.SalvarArquivo(conteudo, "Contrato_teste", ".pdf");

        Assert.True(File.Exists(caminho));
        Assert.Equal(conteudo, File.ReadAllBytes(caminho));
    }

    public void Dispose()
    {
        if (Directory.Exists(_pastaTemporaria))
            Directory.Delete(_pastaTemporaria, recursive: true);
    }
}