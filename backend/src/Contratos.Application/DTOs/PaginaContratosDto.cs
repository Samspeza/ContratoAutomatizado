namespace Contratos.Application.DTOs;

public sealed record PaginaContratosDto(
    IReadOnlyList<ContratoResumoDto> Itens,
    int Total,
    int Pagina,
    int Tamanho);