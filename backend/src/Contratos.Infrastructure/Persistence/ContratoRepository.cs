using Contratos.Application.Interfaces;
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
}