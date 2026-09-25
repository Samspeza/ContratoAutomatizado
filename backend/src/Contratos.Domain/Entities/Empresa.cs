namespace Contratos.Domain.Entities;

public sealed class Empresa
{
    public required string Cnpj { get; init; }
    public required string RazaoSocial { get; init; }
    public string? NomeFantasia { get; init; }
    public required string Logradouro { get; init; }
    public string? Numero { get; init; }
    public string? Complemento { get; init; }
    public required string Bairro { get; init; }
    public required string Municipio { get; init; }
    public required string Uf { get; init; }
    public required string Cep { get; init; }
    public string? SituacaoCadastral { get; init; }
}