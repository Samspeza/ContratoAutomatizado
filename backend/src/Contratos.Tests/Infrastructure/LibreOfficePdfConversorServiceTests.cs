using Contratos.Infrastructure.Documents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class LibreOfficePdfConversorServiceTests
{
    // Este teste é de integração: depende do LibreOffice estar instalado
    // no caminho configurado em appsettings.json (Etapa 9).
    // Se der erro de "arquivo não encontrado", ajuste o caminho abaixo
    // para o mesmo valor usado em appsettings.json na sua máquina.
    private const string CaminhoLibreOffice = @"C:\Program Files\LibreOffice\program\soffice.exe";

    [Fact]
    public void ConverterDocxParaPdf_ComDocxValido_DeveRetornarBytesDeUmPdf()
    {
        if (!File.Exists(CaminhoLibreOffice))
        {
            // Ambiente sem LibreOffice instalado (ex.: outra estação de desenvolvimento) —
            // não falha o build, mas também não valida a conversão de verdade.
            return;
        }

        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Pdf:CaminhoExecutavelLibreOffice"] = CaminhoLibreOffice,
                ["Pdf:TimeoutSegundos"] = "30"
            })
            .Build();

        var servico = new LibreOfficePdfConversorService(configuracao, NullLogger<LibreOfficePdfConversorService>.Instance);

        var docxMinimo = CriarDocxMinimoParaTeste();
        var pdfGerado = servico.ConverterDocxParaPdf(docxMinimo);

        Assert.NotEmpty(pdfGerado);
        // Todo PDF válido começa com essa assinatura de arquivo ("%PDF-")
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(pdfGerado, 0, 5));
    }

    private static byte[] CriarDocxMinimoParaTeste()
    {
        using var memoryStream = new MemoryStream();
        using (var documento = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Create(
            memoryStream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var mainPart = documento.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(
                new DocumentFormat.OpenXml.Wordprocessing.Body(
                    new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                        new DocumentFormat.OpenXml.Wordprocessing.Run(
                            new DocumentFormat.OpenXml.Wordprocessing.Text("Documento de teste.")))));
            mainPart.Document.Save();
        }
        return memoryStream.ToArray();
    }
}