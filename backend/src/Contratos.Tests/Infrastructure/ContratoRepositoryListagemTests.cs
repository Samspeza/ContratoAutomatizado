using Contratos.Application.Queries;
using Contratos.Domain.Entities;
using Contratos.Domain.Enums;
using Contratos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class ContratoRepositoryListagemTests : IDisposable
{
    private readonly string _caminhoBanco;
    private readonly ContratosDbContext _contexto;
    private readonly ContratoRepository _repositorio;

    public ContratoRepositoryListagemTests()
    {
        _caminhoBanco = Path.Combine(Path.GetTempPath(), $"contratos_listagem_{Guid.NewGuid()}.db");

        var opcoes = new DbContextOptionsBuilder<ContratosDbContext>()
            .UseSqlite($"Data Source={_caminhoBanco}")
            .Options;

        _contexto = new ContratosDbContext(opcoes);
        _contexto.Database.EnsureCreated();
        _repositorio = new ContratoRepository(_contexto);
    }

    private async Task<int> CriarAsync(string razaoSocial, string cnpj, DateTime criadoEm, Action<Contrato>? ajustar = null)
    {
        var contrato = Contrato.CriarRascunho(
            cnpj, razaoSocial, "Rua Teste, 100", 25, 3, new DateOnly(2026, 10, 29),
            new List<ContratoResponsavel>());
        contrato.CriadoEm = criadoEm;
        ajustar?.Invoke(contrato);
        return await _repositorio.CriarAsync(contrato);
    }

    private static FiltroContratos SemFiltro(int pagina = 1, int tamanho = 20) =>
        new(null, null, null, null, pagina, tamanho);

    [Fact]
    public async Task Listar_SemFiltro_DeveOrdenarDoMaisRecenteParaOMaisAntigo()
    {
        await CriarAsync("ANTIGA LTDA", "11111111000191", new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        await CriarAsync("RECENTE LTDA", "22222222000191", new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc));

        var pagina = await _repositorio.ListarAsync(SemFiltro());

        Assert.Equal(2, pagina.Total);
        Assert.Equal("RECENTE LTDA", pagina.Itens[0].RazaoSocial);
    }

    [Fact]
    public async Task Listar_ComBuscaSemAcento_DeveEncontrarRazaoSocialComAcento()
    {
        await CriarAsync("SÃO JOSÉ COMÉRCIO LTDA", "11111111000191", DateTime.UtcNow);
        await CriarAsync("OUTRA EMPRESA LTDA", "22222222000191", DateTime.UtcNow);

        var pagina = await _repositorio.ListarAsync(SemFiltro() with { Busca = "sao jose" });

        Assert.Single(pagina.Itens);
        Assert.Equal("SÃO JOSÉ COMÉRCIO LTDA", pagina.Itens[0].RazaoSocial);
    }

    [Fact]
    public async Task Listar_ComBuscaPorCnpjComMascara_DeveEncontrar()
    {
        await CriarAsync("EMPRESA A LTDA", "12345678000190", DateTime.UtcNow);
        await CriarAsync("EMPRESA B LTDA", "99999999000191", DateTime.UtcNow);

        var pagina = await _repositorio.ListarAsync(SemFiltro() with { Busca = "12.345.678/0001-90" });

        Assert.Single(pagina.Itens);
        Assert.Equal("EMPRESA A LTDA", pagina.Itens[0].RazaoSocial);
    }

    [Fact]
    public async Task Listar_ComFiltroDeStatus_DeveRetornarSoOStatusPedido()
    {
        await CriarAsync("GERADA LTDA", "11111111000191", DateTime.UtcNow, c => c.MarcarComoGerado("a.docx", "a.pdf"));
        await CriarAsync("RASCUNHO LTDA", "22222222000191", DateTime.UtcNow);

        var pagina = await _repositorio.ListarAsync(SemFiltro() with { Status = StatusContrato.Gerado });

        Assert.Single(pagina.Itens);
        Assert.Equal("GERADA LTDA", pagina.Itens[0].RazaoSocial);
    }

    [Fact]
    public async Task Listar_ComPeriodo_DeveRespeitarOsLimites()
    {
        await CriarAsync("ANTIGA LTDA", "11111111000191", new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc));
        await CriarAsync("RECENTE LTDA", "22222222000191", new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc));

        var filtro = SemFiltro() with
        {
            CriadoDesdeUtc = new DateTime(2026, 10, 10, 0, 0, 0, DateTimeKind.Utc),
            CriadoAteUtcExclusivo = new DateTime(2026, 10, 25, 0, 0, 0, DateTimeKind.Utc)
        };

        var pagina = await _repositorio.ListarAsync(filtro);

        Assert.Single(pagina.Itens);
        Assert.Equal("RECENTE LTDA", pagina.Itens[0].RazaoSocial);
    }

    [Fact]
    public async Task Listar_ComPaginacao_DeveDevolverATerceiraPaginaECorretoTotal()
    {
        for (var i = 1; i <= 5; i++)
            await CriarAsync($"EMPRESA {i} LTDA", $"1111111100019{i}", new DateTime(2026, 10, i, 12, 0, 0, DateTimeKind.Utc));

        var pagina = await _repositorio.ListarAsync(SemFiltro(pagina: 3, tamanho: 2));

        Assert.Equal(5, pagina.Total);
        Assert.Single(pagina.Itens);
        Assert.Equal("EMPRESA 1 LTDA", pagina.Itens[0].RazaoSocial);
    }

    public void Dispose()
    {
        _contexto.Dispose();
        if (File.Exists(_caminhoBanco)) File.Delete(_caminhoBanco);
    }
}