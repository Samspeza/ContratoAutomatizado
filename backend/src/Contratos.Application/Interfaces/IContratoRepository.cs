using Contratos.Domain.Entities;

namespace Contratos.Application.Interfaces;

public interface IContratoRepository
{
    Task<int> CriarAsync(Contrato contrato);
    Task<Contrato?> ObterPorIdAsync(int id);
    Task AtualizarAsync(Contrato contrato);
}