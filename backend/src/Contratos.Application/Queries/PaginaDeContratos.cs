using Contratos.Domain.Entities;

namespace Contratos.Application.Queries;

public sealed record PaginaDeContratos(IReadOnlyList<Contrato> Itens, int Total);