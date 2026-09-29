using System.Globalization;
using Contratos.Application.DTOs;
using Contratos.Application.Formatting;
using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Storage;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Documents;

public sealed class OpenXmlGeradorDocumentoService : IGeradorDocumentoService
{
    private readonly string _caminhoTemplate;

    public OpenXmlGeradorDocumentoService(IConfiguration configuration)
    {
        var caminhoConfigurado = configuration["Documentos:CaminhoTemplate"];

        _caminhoTemplate = string.IsNullOrWhiteSpace(caminhoConfigurado)
            ? Path.Combine(new PastasAplicacao(configuration).Templates, TemplatePadraoInicializador.NomeArquivoTemplate)
            : caminhoConfigurado;
    }

    public byte[] GerarContrato(DadosContratoDto dados)
    {
        if (!File.Exists(_caminhoTemplate))
            throw new FileNotFoundException("Template de contrato não encontrado.", _caminhoTemplate);

        using var memoryStream = new MemoryStream();
        using (var arquivoTemplate = File.OpenRead(_caminhoTemplate))
        {
            arquivoTemplate.CopyTo(memoryStream);
        }
        memoryStream.Position = 0;

        var valores = MontarValoresDosPlaceholders(dados);

        using (var documento = WordprocessingDocument.Open(memoryStream, isEditable: true))
        {
            var corpo = documento.MainDocumentPart?.Document?.Body
                ?? throw new InvalidOperationException("Documento de template inválido: corpo não encontrado.");

            foreach (var run in corpo.Descendants<Run>())
            {
                var textoElemento = run.GetFirstChild<Text>();
                if (textoElemento is null) continue;

                if (valores.TryGetValue(textoElemento.Text, out var valorSubstituido))
                {
                    textoElemento.Text = valorSubstituido;
                    textoElemento.Space = DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve;

                    RemoverRealceDoRun(run);
                    RemoverRealceDoParagrafo(run.Parent as Paragraph);
                }
            }

            documento.MainDocumentPart!.Document.Save();
        }

        return memoryStream.ToArray();
    }

    private static void RemoverRealceDoRun(Run run)
    {
        var destaque = run.RunProperties?.GetFirstChild<Highlight>();
        destaque?.Remove();
    }

    private static void RemoverRealceDoParagrafo(Paragraph? paragrafo)
    {
        var destaqueParagrafo = paragrafo?.ParagraphProperties?.ParagraphMarkRunProperties?.GetFirstChild<Highlight>();
        destaqueParagrafo?.Remove();
    }

    private static Dictionary<string, string> MontarValoresDosPlaceholders(DadosContratoDto dados)
    {
        var endereco = EnderecoFormatter.Formatar(dados.Empresa);

        var responsaveis = dados.Responsaveis;
        var responsavel1 = responsaveis.Count > 0 ? FormatarResponsavel(responsaveis[0]) : string.Empty;
        var responsavel2 = responsaveis.Count > 1 ? FormatarResponsavel(responsaveis[1]) : string.Empty;

        var cultura = new CultureInfo("pt-BR");

        return new Dictionary<string, string>
        {
            ["{{RAZAO_SOCIAL_CONTRATANTE}}"] = dados.Empresa.RazaoSocial,
            ["{{CNPJ_CONTRATANTE}}"] = $"CNPJ nº {CnpjFormatter.Formatar(dados.Empresa.Cnpj)}",
            ["{{ENDERECO_CONTRATANTE}}"] = endereco,
            ["{{COMPLEMENTO_ENDERECO_CONTRATANTE}}"] = dados.ComplementoEndereco ?? string.Empty,
            ["{{RESPONSAVEL_1}}"] = responsavel1,
            ["{{RESPONSAVEL_2}}"] = responsavel2,
            ["{{DATA_CONTRATO_NUMERICA}}"] = $"Data: {dados.DataContrato:dd/MM/yyyy}",
            ["{{DATA_CONTRATO_EXTENSO}}"] = $"{dados.DataContrato!.Value.ToString("d 'de' MMMM 'de' yyyy", cultura)}.",
            ["{{PERCENTUAL_HONORARIOS}}"] = NumeroPorExtenso.FormatarPercentual(dados.PercentualHonorarios),
            ["{{QUANTIDADE_PARCELAS}}"] = NumeroPorExtenso.FormatarParcelas(dados.QuantidadeParcelas)
        };
    }

    private static string FormatarResponsavel(ResponsavelAssinaturaDto responsavel)
    {
        return string.IsNullOrWhiteSpace(responsavel.Cpf)
            ? $"Att. Sr(a). {responsavel.Nome}"
            : $"Att. Sr(a). {responsavel.Nome} - CPF {responsavel.Cpf}";
    }
}