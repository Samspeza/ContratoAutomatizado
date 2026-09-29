using Contratos.Domain.Enums;

namespace Contratos.Domain.Entities;

public sealed class EventoHistorico
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public DateTime DataHora { get; set; }
    public TipoEventoHistorico Tipo { get; set; }
    public string Descricao { get; set; } = string.Empty;

    // Sem autenticação multiusuário ainda — deixado pronto para o futuro (seção 26 da especificação).
    public string? Usuario { get; set; }
}