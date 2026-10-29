using Contratos.Domain.Enums;

namespace Contratos.Application.Queries;

public sealed record FiltroContratos(
    string? Busca,
    StatusContrato? Status,
    DateTime? CriadoDesdeUtc,
    DateTime? CriadoAteUtcExclusivo,
    int Pagina,
    int Tamanho);