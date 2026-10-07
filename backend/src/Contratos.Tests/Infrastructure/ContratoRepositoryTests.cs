using Contratos.Domain.Entities;
using Contratos.Domain.Enums;
using Contratos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class ContratoRepositoryTests : IDisposable
{
    private readonly string _caminhoBanco;
    private readonly ContratosDbContext _contexto;
    private readonly ContratoRepository _repositorio;

    public ContratoRepositoryTests()
    {
        _caminhoBanco = Path.Combine(Path.GetTempPath(), $"contratos_teste_{Guid.NewGuid()}.db");

        var opcoes = new DbContextOptionsBuilder<ContratosDbContext>()
            .UseSqlite($"Data Source={_caminhoBanco}")
            .Options;

        _contexto = new ContratosDbContext(opcoes);
        _contexto.Database.EnsureCreated();
        _repositorio = new ContratoRepository(_contexto);
    }

    [Fact]
    public async Task CriarEObterPorId_DevePersistirContratoComResponsaveisEHistorico()
    {
        var contrato = Contrato.CriarRascunho(
            "12345678000190", "EMPRESA TESTE LTDA", "Rua Teste, 100",
            25, 3, new DateOnly(2026, 9, 29),
            new List<ContratoResponsavel>
            {
                new() { Nome = "Fulano de Tal", Cpf = "111.444.777-35" },
                new() { Nome = "Ciclano" }
            });

        var id = await _repositorio.CriarAsync(contrato);

        var recuperado = await _repositorio.ObterPorIdAsync(id);

        Assert.NotNull(recuperado);
        Assert.Equal("EMPRESA TESTE LTDA", recuperado!.RazaoSocial);
        Assert.Equal(StatusContrato.Rascunho, recuperado.Status);
        Assert.Equal(2, recuperado.Responsaveis.Count);
        Assert.Single(recuperado.Historico);
        Assert.Equal(TipoEventoHistorico.ContratoCriado, recuperado.Historico[0].Tipo);
    }

    [Fact]
    public async Task Atualizar_DevePersistirNovoStatusEAtualizarData()
    {
        var contrato = Contrato.CriarRascunho(
            "12345678000190", "EMPRESA TESTE LTDA", "Rua Teste, 100",
            25, 3, new DateOnly(2026, 9, 29), new List<ContratoResponsavel>());

        var id = await _repositorio.CriarAsync(contrato);
        var dataOriginal = contrato.AtualizadoEm;

        contrato.MarcarComoGerado("caminho.docx", "caminho.pdf");
        await _repositorio.AtualizarAsync(contrato);

        var recuperado = await _repositorio.ObterPorIdAsync(id);

        Assert.Equal(StatusContrato.Gerado, recuperado!.Status);
        Assert.True(recuperado.AtualizadoEm >= dataOriginal);
        Assert.Equal(2, recuperado.Historico.Count);
    }

    public void Dispose()
    {
        _contexto.Database.EnsureDeleted();
        _contexto.Dispose();

        if (File.Exists(_caminhoBanco))
            File.Delete(_caminhoBanco);
    }
}