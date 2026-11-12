using Contratos.Domain.Entities;
using Contratos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class ContratoRepositoryResumoTests : IDisposable
{
    private readonly string _caminhoBanco;
    private readonly ContratosDbContext _contexto;
    private readonly ContratoRepository _repositorio;

    public ContratoRepositoryResumoTests()
    {
        _caminhoBanco = Path.Combine(Path.GetTempPath(), $"contratos_resumo_{Guid.NewGuid()}.db");

        var opcoes = new DbContextOptionsBuilder<ContratosDbContext>()
            .UseSqlite($"Data Source={_caminhoBanco}")
            .Options;

        _contexto = new ContratosDbContext(opcoes);
        _contexto.Database.EnsureCreated();
        _repositorio = new ContratoRepository(_contexto);
    }

    private static Contrato Novo(string razaoSocial) =>
        Contrato.CriarRascunho(
            "12345678000190", razaoSocial, "Rua Teste, 100", 25, 3, new DateOnly(2026, 11, 3),
            new List<ContratoResponsavel>());

    private static Contrato Gerado(string razaoSocial)
    {
        var contrato = Novo(razaoSocial);
        contrato.MarcarComoGerado("a.docx", "a.pdf");
        return contrato;
    }

    private static Contrato Pronto(string razaoSocial)
    {
        var contrato = Gerado(razaoSocial);
        contrato.AvancarParaEnvio();
        contrato.DefinirEmailDestinatario("cliente@empresa.com.br");
        return contrato;
    }

    private async Task SalvarAsync(Contrato contrato, DateTime criadoEm, DateTime atualizadoEm)
    {
        contrato.CriadoEm = criadoEm;
        contrato.AtualizadoEm = atualizadoEm;
        await _repositorio.CriarAsync(contrato);
    }

    [Fact]
    public async Task ObterResumo_DeveContarCadaIndicadorEListarPendenciasDoMaisAntigoAoMaisNovo()
    {
        var agora = DateTime.UtcNow;

        await SalvarAsync(Gerado("A GERADO HOJE"), agora, agora);

        await SalvarAsync(Pronto("B PRONTO PARADO"), agora.AddDays(-6), agora.AddDays(-5));

        var concluido = Pronto("C CONCLUIDO HOJE");
        concluido.MarcarEmailEnviado();
        await SalvarAsync(concluido, agora.AddDays(-2), agora);

        var comErro = Pronto("D COM ERRO");
        comErro.MarcarErroEnvioEmail("falha");
        await SalvarAsync(comErro, agora.AddDays(-1), agora);

        var cancelado = Pronto("E CANCELADO");
        cancelado.Cancelar();
        await SalvarAsync(cancelado, agora.AddDays(-10), agora.AddDays(-10));

        await SalvarAsync(Novo("F RASCUNHO PARADO"), agora.AddDays(-4), agora.AddDays(-4));

        var resumo = await _repositorio.ObterResumoAsync(
            inicioDoDiaUtc: agora.AddHours(-12),
            paradosAteUtc: agora.AddDays(-3),
            maximoPendencias: 10);

        Assert.Equal(1, resumo.CriadosHoje);
        Assert.Equal(1, resumo.EnviadosHoje);
        Assert.Equal(2, resumo.AguardandoEnvio);   // A e B
        Assert.Equal(1, resumo.ComErro);           // D
        Assert.Equal(3, resumo.TotalPendencias);   // B (parado), F (rascunho parado), D (erro)
        Assert.Equal(
            new[] { "B PRONTO PARADO", "F RASCUNHO PARADO", "D COM ERRO" },
            resumo.Pendencias.Select(c => c.RazaoSocial).ToArray());
    }

    [Fact]
    public async Task ObterResumo_DeveLimitarAListaMasInformarOTotalDePendencias()
    {
        var agora = DateTime.UtcNow;

        for (var i = 1; i <= 3; i++)
        {
            var comErro = Pronto($"ERRO {i}");
            comErro.MarcarErroEnvioEmail("falha");
            await SalvarAsync(comErro, agora.AddDays(-i), agora.AddDays(-i));
        }

        var resumo = await _repositorio.ObterResumoAsync(agora.AddHours(-12), agora.AddDays(-3), maximoPendencias: 2);

        Assert.Equal(3, resumo.TotalPendencias);
        Assert.Equal(2, resumo.Pendencias.Count);
        Assert.Equal("ERRO 3", resumo.Pendencias[0].RazaoSocial); // o mais antigo primeiro
    }

    [Fact]
    public async Task ObterResumo_SemContratos_DeveRetornarTudoZerado()
    {
        var agora = DateTime.UtcNow;

        var resumo = await _repositorio.ObterResumoAsync(agora.AddHours(-12), agora.AddDays(-3), 8);

        Assert.Equal(0, resumo.CriadosHoje);
        Assert.Equal(0, resumo.EnviadosHoje);
        Assert.Equal(0, resumo.AguardandoEnvio);
        Assert.Equal(0, resumo.ComErro);
        Assert.Equal(0, resumo.TotalPendencias);
        Assert.Empty(resumo.Pendencias);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        if (File.Exists(_caminhoBanco)) File.Delete(_caminhoBanco);
    }
}