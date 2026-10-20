using System.Globalization;
using System.Text.RegularExpressions;
using Contratos.Application.Formatting;
using Contratos.Domain.Entities;

namespace Contratos.Application.Messages;

public static class PlaceholdersMensagem
{
    private static readonly Regex Padrao = new(@"\{\{([A-Z_]+)\}\}", RegexOptions.Compiled);

    public static IReadOnlyDictionary<string, string> Montar(Contrato contrato) =>
        new Dictionary<string, string>
        {
            ["RAZAO_SOCIAL"] = contrato.RazaoSocial,
            ["CNPJ"] = CnpjFormatter.Formatar(contrato.Cnpj),
            ["NOME_RESPONSAVEL"] = contrato.Responsaveis.FirstOrDefault()?.Nome ?? string.Empty,
            ["DATA_CONTRATO"] = contrato.DataContrato.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
        };

    public static string Substituir(string texto, IReadOnlyDictionary<string, string> valores)
    {
        return Padrao.Replace(texto, correspondencia =>
            valores.TryGetValue(correspondencia.Groups[1].Value, out var valor)
                ? valor
                : throw new InvalidOperationException(
                    $"A mensagem padrão usa um campo desconhecido: {correspondencia.Value}. Corrija o texto da mensagem antes de enviar."));
    }
}