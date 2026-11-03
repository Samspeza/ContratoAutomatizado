using Contratos.Domain.Entities;
using Contratos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class ContratoRepositoryFluxoTests : IDisposable
{
    private readonly string _caminhoBanco;
    private readonly ContratosDbContext _contexto;
    private readonly ContratoRepository _repositorio;

    public ContratoRepositoryFluxoTests()
    {
        _caminhoBanco = Path.Combine(Path.GetTempPath(), $"contratos_fluxo_{Guid.NewGuid()}.db");

        var opcoes = new DbContextOptionsBuilder<ContratosDbContext>()
            .UseSqlite($"Data Source={_caminhoBanco}")
            .Options;

        _contexto = new ContratosDbContext(opcoes);
        _contexto.Database.EnsureCreated();
        _repositorio = new ContratoRepository(_contexto);
    }

    private static Contrato NovoContrato(string razaoSocial) =>
        Contrato.CriarRascunho(
            "12345678000190", razaoSocial, "Rua Teste, 100", 25, 3, new DateOnly(2026, 11, 3),
            new List<ContratoResponsavel>());

    private static Contrato Gerado(string razaoSocial)
    {
        var contrato = NovoContrato(razaoSocial);
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

    [Fact]
    public async Task ListarParaFluxo_DeveTrazerEmAbertoEConcluidosRecentes_EIgnorarOResto()
    {
        var gerado = Gerado("GERADO LTDA");

        var pronto = Pronto("PRONTO LTDA");

        var comErro = Pronto("ERRO LTDA");
        comErro.MarcarErroEnvioEmail("falha");

        var concluidoRecente = Pronto("CONCLUIDO RECENTE LTDA");
        concluidoRecente.MarcarEmailEnviado();

        var concluidoAntigo = Pronto("CONCLUIDO ANTIGO LTDA");
        concluidoAntigo.MarcarEmailEnviado();
        concluidoAntigo.EmailEnviadoEm = DateTime.UtcNow.AddDays(-30);

        var cancelado = Pronto("CANCELADO LTDA");
        cancelado.Cancelar();

        foreach (var contrato in new[] { gerado, pronto, comErro, concluidoRecente, concluidoAntigo, cancelado })
            await _repositorio.CriarAsync(contrato);

        var resultado = await _repositorio.ListarParaFluxoAsync(DateTime.UtcNow.AddDays(-7));

        var nomes = resultado.Select(c => c.RazaoSocial).ToHashSet();
        Assert.Equal(
            new HashSet<string> { "GERADO LTDA", "PRONTO LTDA", "ERRO LTDA", "CONCLUIDO RECENTE LTDA" },
            nomes);
    }

    [Fact]
    public async Task ListarParaFluxo_ComPeriodoMaior_DeveIncluirConcluidosMaisAntigos()
    {
        var concluidoAntigo = Pronto("CONCLUIDO ANTIGO LTDA");
        concluidoAntigo.MarcarEmailEnviado();
        concluidoAntigo.EmailEnviadoEm = DateTime.UtcNow.AddDays(-30);
        await _repositorio.CriarAsync(concluidoAntigo);

        var em7Dias = await _repositorio.ListarParaFluxoAsync(DateTime.UtcNow.AddDays(-7));
        var em90Dias = await _repositorio.ListarParaFluxoAsync(DateTime.UtcNow.AddDays(-90));

        Assert.Empty(em7Dias);
        Assert.Single(em90Dias);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        if (File.Exists(_caminhoBanco)) File.Delete(_caminhoBanco);
    }
}