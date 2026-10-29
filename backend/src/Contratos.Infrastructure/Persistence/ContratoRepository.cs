using System.Globalization;
using System.Text;
using Contratos.Application.Interfaces;
using Contratos.Application.Queries;
using Contratos.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Contratos.Infrastructure.Persistence;

public sealed class ContratoRepository : IContratoRepository
{
    private readonly ContratosDbContext _contexto;

    public ContratoRepository(ContratosDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<int> CriarAsync(Contrato contrato)
    {
        _contexto.Contratos.Add(contrato);
        await _contexto.SaveChangesAsync();
        return contrato.Id;
    }

    public async Task<Contrato?> ObterPorIdAsync(int id)
    {
        return await _contexto.Contratos
            .Include(c => c.Responsaveis)
            .Include(c => c.Historico)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AtualizarAsync(Contrato contrato)
    {
        contrato.AtualizadoEm = DateTime.UtcNow;
        _contexto.Contratos.Update(contrato);
        await _contexto.SaveChangesAsync();
    }

    public async Task<PaginaDeContratos> ListarAsync(FiltroContratos filtro)
    {
        var consulta = _contexto.Contratos.AsNoTracking().AsQueryable();

        if (filtro.Status.HasValue)
            consulta = consulta.Where(c => c.Status == filtro.Status.Value);

        if (filtro.CriadoDesdeUtc.HasValue)
            consulta = consulta.Where(c => c.CriadoEm >= filtro.CriadoDesdeUtc.Value);

        if (filtro.CriadoAteUtcExclusivo.HasValue)
            consulta = consulta.Where(c => c.CriadoEm < filtro.CriadoAteUtcExclusivo.Value);

        var candidatos = await consulta
            .OrderByDescending(c => c.CriadoEm)
            .ThenByDescending(c => c.Id)
            .ToListAsync();

        // A busca de texto é feita em memória porque o SQLite não ignora acentos ("sao" x "SÃO").
        // Para uso local (milhares de contratos, no máximo) o custo é desprezível.
        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = Normalizar(filtro.Busca.Trim());
            var digitos = new string(filtro.Busca.Where(char.IsDigit).ToArray());

            candidatos = candidatos
                .Where(c => Normalizar(c.RazaoSocial).Contains(termo)
                            || (digitos.Length > 0 && c.Cnpj.Contains(digitos)))
                .ToList();
        }

        var itens = candidatos
            .Skip((filtro.Pagina - 1) * filtro.Tamanho)
            .Take(filtro.Tamanho)
            .ToList();

        return new PaginaDeContratos(itens, candidatos.Count);
    }

    private static string Normalizar(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var semAcentos = decomposto.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark);
        return new string(semAcentos.ToArray()).ToUpperInvariant();
    }
}