using Contratos.Application.DTOs;
using Contratos.Domain.Exceptions;
using Contratos.Domain.ValueObjects;

namespace Contratos.Application.Validation;

public static class DadosContratoValidator
{
    private const decimal PercentualMinimo = 0.01m;
    private const decimal PercentualMaximo = 100m;
    private const int ParcelasMinimo = 1;
    private const int ParcelasMaximo = 60;
    private const int ResponsaveisMinimo = 1;
    private const int ResponsaveisMaximo = 2;

    public static void Validar(DadosContratoDto dados)
    {
        var mensagens = new List<string>();

        if (dados.Empresa is null || string.IsNullOrWhiteSpace(dados.Empresa.Cnpj))
            mensagens.Add("Os dados da empresa são obrigatórios.");

        if (dados.PercentualHonorarios < PercentualMinimo || dados.PercentualHonorarios > PercentualMaximo)
            mensagens.Add($"O percentual de honorários deve estar entre {PercentualMinimo}% e {PercentualMaximo}%.");

        if (dados.QuantidadeParcelas < ParcelasMinimo || dados.QuantidadeParcelas > ParcelasMaximo)
            mensagens.Add($"A quantidade de parcelas deve estar entre {ParcelasMinimo} e {ParcelasMaximo}.");

        if (dados.DataContrato == default)
            mensagens.Add("A data do contrato é obrigatória.");

        if (dados.Responsaveis is null || dados.Responsaveis.Count < ResponsaveisMinimo)
            mensagens.Add("É necessário informar ao menos um responsável pela assinatura.");
        else if (dados.Responsaveis.Count > ResponsaveisMaximo)
            mensagens.Add($"São permitidos no máximo {ResponsaveisMaximo} responsáveis pela assinatura.");
        else
        {
            foreach (var responsavel in dados.Responsaveis)
            {
                if (string.IsNullOrWhiteSpace(responsavel.Nome))
                    mensagens.Add("O nome do responsável pela assinatura é obrigatório.");

                if (!Cpf.EhValido(responsavel.Cpf))
                    mensagens.Add($"CPF inválido para o responsável \"{responsavel.Nome}\".");
            }
        }

        if (mensagens.Count > 0)
            throw new DadosContratoInvalidosException(mensagens);
    }
}