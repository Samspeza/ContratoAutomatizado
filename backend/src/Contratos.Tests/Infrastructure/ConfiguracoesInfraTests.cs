using System.Text;
using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Configuracoes;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class ConfiguracoesUsuarioServiceTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "ContratoAutomatizado_Testes_" + Guid.NewGuid());
    private readonly IConfigurationRoot _configuracao;
    private readonly ConfiguracoesUsuarioService _servico;

    public ConfiguracoesUsuarioServiceTests()
    {
        var pastaConfiguracoes = Path.Combine(_raiz, "Configuracoes");
        Directory.CreateDirectory(pastaConfiguracoes);

        _configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Armazenamento:PastaRaiz"] = _raiz,
                ["Acompanhamento:DiasParado"] = "3"
            })
            .AddJsonFile(Path.Combine(pastaConfiguracoes, "configuracoes-usuario.json"), optional: true, reloadOnChange: false)
            .Build();

        _servico = new ConfiguracoesUsuarioService(_configuracao);
    }

    [Fact]
    public void Definir_DeveValerImediatamenteNaConfiguracao()
    {
        _servico.Definir("Acompanhamento:DiasParado", "5");

        Assert.Equal("5", _configuracao["Acompanhamento:DiasParado"]);
    }

    [Fact]
    public void Definir_ComValorVazio_DeveRemoverAPersonalizacaoEVoltarAoPadrao()
    {
        _servico.Definir("Acompanhamento:DiasParado", "5");

        _servico.Definir("Acompanhamento:DiasParado", null);

        Assert.Equal("3", _configuracao["Acompanhamento:DiasParado"]);
    }

    [Fact]
    public void Definir_DeveManterAsOutrasChavesJaSalvas()
    {
        _servico.Definir("Armazenamento:PastaContratosGerados", @"D:\Contratos");
        _servico.Definir("Acompanhamento:DiasParado", "5");

        _servico.Definir("Acompanhamento:DiasParado", null);

        Assert.Equal(@"D:\Contratos", _configuracao["Armazenamento:PastaContratosGerados"]);
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
    }
}

public class SegredosDpapiServiceTests : IDisposable
{
    private readonly string _raiz = Path.Combine(Path.GetTempPath(), "ContratoAutomatizado_Testes_" + Guid.NewGuid());

    private IConfiguration CriarConfiguracao(string? senhaDaConfiguracao = null)
    {
        var valores = new Dictionary<string, string?> { ["Armazenamento:PastaRaiz"] = _raiz };
        if (senhaDaConfiguracao is not null) valores["Envio:Email:Smtp:Senha"] = senhaDaConfiguracao;

        return new ConfigurationBuilder().AddInMemoryCollection(valores).Build();
    }

    [Fact]
    public void DefinirSenha_DeveGuardarProtegida_EDevolverAMesmaSenha()
    {
        if (!OperatingSystem.IsWindows()) return;

        var servico = new SegredosDpapiService(CriarConfiguracao());

        servico.DefinirSenhaSmtp("senha-super-secreta");

        Assert.Equal("senha-super-secreta", servico.ObterSenhaSmtp());
        Assert.Equal(OrigemSenha.ArmazenamentoSeguro, servico.ObterOrigemSenhaSmtp());

        var conteudoDoArquivo = File.ReadAllBytes(Path.Combine(_raiz, "Configuracoes", "email-senha.protegida"));
        Assert.DoesNotContain("senha-super-secreta", Encoding.UTF8.GetString(conteudoDoArquivo));
    }

    [Fact]
    public void SemSenhaGuardada_DeveUsarADaConfiguracao_ESenaoHouverNenhuma()
    {
        if (!OperatingSystem.IsWindows()) return;

        var comConfiguracao = new SegredosDpapiService(CriarConfiguracao("da-configuracao"));
        Assert.Equal(OrigemSenha.Configuracao, comConfiguracao.ObterOrigemSenhaSmtp());
        Assert.Equal("da-configuracao", comConfiguracao.ObterSenhaSmtp());

        var semNada = new SegredosDpapiService(CriarConfiguracao());
        Assert.Equal(OrigemSenha.Nenhuma, semNada.ObterOrigemSenhaSmtp());
        Assert.Null(semNada.ObterSenhaSmtp());
    }

    [Fact]
    public void SenhaGuardada_DevePrevalecerSobreADaConfiguracao_ERemoverVoltaParaAConfiguracao()
    {
        if (!OperatingSystem.IsWindows()) return;

        var servico = new SegredosDpapiService(CriarConfiguracao("da-configuracao"));

        servico.DefinirSenhaSmtp("guardada");
        Assert.Equal("guardada", servico.ObterSenhaSmtp());

        servico.RemoverSenhaSmtp();
        Assert.Equal("da-configuracao", servico.ObterSenhaSmtp());
        Assert.Equal(OrigemSenha.Configuracao, servico.ObterOrigemSenhaSmtp());
    }

    public void Dispose()
    {
        if (Directory.Exists(_raiz)) Directory.Delete(_raiz, recursive: true);
    }
}