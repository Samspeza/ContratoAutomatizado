using System.Globalization;
using System.Text.RegularExpressions;
using Contratos.Application.Formatting;
using Contratos.Domain.Entities;

namespace Contratos.Application.Messages;

public sealed record InformacaoPlaceholder(string Nome, string Descricao, string Exemplo);

public static class PlaceholdersMensagem
{
    // Qualquer texto entre chaves duplas é tratado como campo. Se o nome não existir, é erro:
    // um "{{...}}" nunca pode seguir literalmente para o cliente.
    private static readonly Regex Padrao = new(@"\{\{([^{}]*)\}\}", RegexOptions.Compiled);

    public static IReadOnlyList<InformacaoPlaceholder> Disponiveis { get; } = new[]
    {
        new InformacaoPlaceholder("RAZAO_SOCIAL", "Razão social da empresa contratante", "EMPRESA EXEMPLO LTDA"),
        new InformacaoPlaceholder("CNPJ", "CNPJ da empresa, com máscara", "12.345.678/0001-90"),
        new InformacaoPlaceholder("NOME_RESPONSAVEL", "Nome do primeiro responsável pela assinatura", "Maria da Silva"),
        new InformacaoPlaceholder("DATA_CONTRATO", "Data do contrato (dd/mm/aaaa)", "05/11/2026")
    };

    public static IReadOnlyDictionary<string, string> Montar(Contrato contrato) =>
        new Dictionary<string, string>
        {
            ["RAZAO_SOCIAL"] = contrato.RazaoSocial,
            ["CNPJ"] = CnpjFormatter.Formatar(contrato.Cnpj),
            ["NOME_RESPONSAVEL"] = contrato.Responsaveis.FirstOrDefault()?.Nome ?? string.Empty,
            ["DATA_CONTRATO"] = contrato.DataContrato.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)
        };

    public static IReadOnlyList<string> NomesDesconhecidos(string texto)
    {
        var conhecidos = Disponiveis.Select(p => p.Nome).ToHashSet();

        return Padrao.Matches(texto)
            .Select(m => m.Groups[1].Value)
            .Where(nome => !conhecidos.Contains(nome))
            .Distinct()
            .ToList();
    }

    public static string Substituir(string texto, IReadOnlyDictionary<string, string> valores)
    {
        return Padrao.Replace(texto, correspondencia =>
            valores.TryGetValue(correspondencia.Groups[1].Value, out var valor)
                ? valor
                : throw new InvalidOperationException(
                    $"A mensagem padrão usa um campo desconhecido: {correspondencia.Value}. Corrija o texto da mensagem antes de enviar."));
    }
}