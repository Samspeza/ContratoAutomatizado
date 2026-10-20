using Contratos.Infrastructure.Envio;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class ModeloMensagemArquivoProviderTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "ContratoAutomatizado_Testes_" + Guid.NewGuid());

    private ModeloMensagemArquivoProvider CriarProvider()
    {
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Armazenamento:PastaRaiz"] = _raiz })
            .Build();
        return new ModeloMensagemArquivoProvider(configuracao);
    }

    [Fact]
    public void ObterModeloEmail_SemArquivo_DeveCriarOPadraoComOTextoFornecido()
    {
        var modelo = CriarProvider().ObterModeloEmail();

        Assert.Equal("Carta de Contratação, Portaria CAT 28/2020", modelo.Assunto);
        Assert.StartsWith("Prezado(a),", modelo.Corpo);
        Assert.EndsWith("www.consultoriamello.com.br", modelo.Corpo);
        Assert.True(File.Exists(Path.Combine(_raiz, "Configuracoes", "mensagem-email.txt")));
    }

    [Fact]
    public void ObterModeloEmail_ComArquivoEditado_NaoDeveSobrescrever()
    {
        var provider = CriarProvider();
        provider.ObterModeloEmail(); // cria o arquivo padrão

        var caminho = Path.Combine(_raiz, "Configuracoes", "mensagem-email.txt");
        File.WriteAllText(caminho, "Assunto: Outro assunto\n\nOutro corpo");

        var modelo = provider.ObterModeloEmail();

        Assert.Equal("Outro assunto", modelo.Assunto);
        Assert.Equal("Outro corpo", modelo.Corpo);
    }

    [Fact]
    public void ObterModeloEmail_ComFormatoInvalido_DeveLancarExcecao()
    {
        var provider = CriarProvider();
        provider.ObterModeloEmail();

        var caminho = Path.Combine(_raiz, "Configuracoes", "mensagem-email.txt");
        File.WriteAllText(caminho, "Sem a linha de assunto\nCorpo");

        Assert.Throws<InvalidOperationException>(() => provider.ObterModeloEmail());
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
    }
}