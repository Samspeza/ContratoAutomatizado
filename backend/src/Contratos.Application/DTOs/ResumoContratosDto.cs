namespace Contratos.Application.DTOs;

public sealed record ResumoContratosDto(
    int CriadosHoje,
    int EnviadosHoje,
    int AguardandoEnvio,
    int ComErro,
    int DiasParado,
    int TotalPendencias,
    IReadOnlyList<ContratoResumoDto> Pendencias);