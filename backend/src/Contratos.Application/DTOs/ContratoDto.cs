using Contratos.Application.Formatting;
using Contratos.Domain.Entities;

namespace Contratos.Application.DTOs;

public sealed record ContratoDto(
    int Id,
    string Cnpj,
    string RazaoSocial,
    string Endereco,
    decimal PercentualHonorarios,
    int QuantidadeParcelas,
    DateOnly DataContrato,
    string Status,
    bool ArquivoDisponivel,
    IReadOnlyList<ResponsavelAssinaturaDto> Responsaveis)
{
    public static ContratoDto DeContrato(Contrato contrato) => new(
        contrato.Id,
        CnpjFormatter.Formatar(contrato.Cnpj),
        contrato.RazaoSocial,
        contrato.Endereco,
        contrato.PercentualHonorarios,
        contrato.QuantidadeParcelas,
        contrato.DataContrato,
        contrato.Status.ToString(),
        !string.IsNullOrWhiteSpace(contrato.CaminhoArquivoPdf) && File.Exists(contrato.CaminhoArquivoPdf),
        contrato.Responsaveis.Select(r => new ResponsavelAssinaturaDto(r.Nome, r.Cpf ?? string.Empty)).ToList());
}