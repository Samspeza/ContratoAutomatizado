using Contratos.Application.Queries;
using Contratos.Domain.Entities;

namespace Contratos.Application.Interfaces;

public interface IContratoRepository
{
    Task<int> CriarAsync(Contrato contrato);
    Task<Contrato?> ObterPorIdAsync(int id);
    Task AtualizarAsync(Contrato contrato);
    Task<PaginaDeContratos> ListarAsync(FiltroContratos filtro);
    Task<IReadOnlyList<Contrato>> ListarParaFluxoAsync(DateTime concluidosDesdeUtc);
    Task<ResumoDeContratos> ObterResumoAsync(DateTime inicioDoDiaUtc, DateTime paradosAteUtc, int maximoPendencias);
}