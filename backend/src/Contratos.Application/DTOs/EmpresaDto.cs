using Contratos.Domain.Entities;

namespace Contratos.Application.DTOs;

public sealed record EmpresaDto(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    string Logradouro,
    string? Numero,
    string? Complemento,
    string Bairro,
    string Municipio,
    string Uf,
    string Cep,
    string? SituacaoCadastral)
{
    public static EmpresaDto DeEmpresa(Empresa empresa) => new(
        empresa.Cnpj,
        empresa.RazaoSocial,
        empresa.NomeFantasia,
        empresa.Logradouro,
        empresa.Numero,
        empresa.Complemento,
        empresa.Bairro,
        empresa.Municipio,
        empresa.Uf,
        empresa.Cep,
        empresa.SituacaoCadastral);
}