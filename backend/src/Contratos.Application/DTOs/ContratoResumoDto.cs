using Contratos.Application.Formatting;
using Contratos.Domain.Entities;

namespace Contratos.Application.DTOs;

public sealed record ContratoResumoDto(
    int Id,
    string Cnpj,
    string RazaoSocial,
    DateTime CriadoEm,
    DateOnly DataContrato,
    string Status,
    string StatusEmail)
{
    public static ContratoResumoDto DeContrato(Contrato contrato) => new(
        contrato.Id,
        CnpjFormatter.Formatar(contrato.Cnpj),
        contrato.RazaoSocial,
        contrato.CriadoEm,
        contrato.DataContrato,
        contrato.Status.ToString(),
        contrato.StatusEmail.ToString());
}