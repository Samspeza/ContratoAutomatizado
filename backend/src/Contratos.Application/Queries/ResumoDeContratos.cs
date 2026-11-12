using Contratos.Domain.Entities;

namespace Contratos.Application.Queries;

public sealed record ResumoDeContratos(
    int CriadosHoje,
    int EnviadosHoje,
    int AguardandoEnvio,
    int ComErro,
    int TotalPendencias,
    IReadOnlyList<Contrato> Pendencias);