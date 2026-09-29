namespace Contratos.Domain.Entities;

public sealed class ContratoResponsavel
{
    public int Id { get; set; }
    public int ContratoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Cpf { get; set; }
}