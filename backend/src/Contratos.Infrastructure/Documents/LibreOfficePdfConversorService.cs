using System.Diagnostics;
using Contratos.Application.Interfaces;
using Contratos.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Contratos.Infrastructure.Documents;

public sealed class LibreOfficePdfConversorService : IPdfConversorService
{
    private readonly string _caminhoExecutavel;
    private readonly TimeSpan _timeout;
    private readonly ILogger<LibreOfficePdfConversorService> _logger;

    public LibreOfficePdfConversorService(IConfiguration configuration, ILogger<LibreOfficePdfConversorService> logger)
    {
        _caminhoExecutavel = configuration["Pdf:CaminhoExecutavelLibreOffice"]
            ?? throw new InvalidOperationException("Configuração 'Pdf:CaminhoExecutavelLibreOffice' não encontrada.");

        var timeoutSegundos = configuration.GetValue<int?>("Pdf:TimeoutSegundos") ?? 30;
        _timeout = TimeSpan.FromSeconds(timeoutSegundos);
        _logger = logger;
    }

    public byte[] ConverterDocxParaPdf(byte[] conteudoDocx)
    {
        if (!File.Exists(_caminhoExecutavel))
        {
            throw new ConversaoPdfIndisponivelException(
                "Não foi possível gerar o PDF: o LibreOffice não foi encontrado neste computador.");
        }

        var pastaTemporaria = Path.Combine(Path.GetTempPath(), "ContratoAutomatizado_" + Guid.NewGuid());
        var pastaPerfilLibreOffice = Path.Combine(pastaTemporaria, "perfil");
        Directory.CreateDirectory(pastaTemporaria);
        Directory.CreateDirectory(pastaPerfilLibreOffice);

        var caminhoDocxTemp = Path.Combine(pastaTemporaria, "documento.docx");

        try
        {
            File.WriteAllBytes(caminhoDocxTemp, conteudoDocx);

            var processInfo = new ProcessStartInfo
            {
                FileName = _caminhoExecutavel,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            processInfo.ArgumentList.Add("--headless");
            processInfo.ArgumentList.Add("--norestore");
            processInfo.ArgumentList.Add($"-env:UserInstallation=file:///{pastaPerfilLibreOffice.Replace('\\', '/')}");
            processInfo.ArgumentList.Add("--convert-to");
            processInfo.ArgumentList.Add("pdf");
            processInfo.ArgumentList.Add("--outdir");
            processInfo.ArgumentList.Add(pastaTemporaria);
            processInfo.ArgumentList.Add(caminhoDocxTemp);

            using var processo = Process.Start(processInfo)
                ?? throw new ConversaoPdfIndisponivelException("Não foi possível iniciar o conversor de PDF.");

            var finalizouNoPrazo = processo.WaitForExit((int)_timeout.TotalMilliseconds);

            if (!finalizouNoPrazo)
            {
                processo.Kill(entireProcessTree: true);
                throw new ConversaoPdfIndisponivelException(
                    "A geração do PDF demorou mais do que o esperado e foi cancelada. Tente novamente.");
            }

            var caminhoPdfGerado = Path.Combine(pastaTemporaria, "documento.pdf");

            if (processo.ExitCode != 0 || !File.Exists(caminhoPdfGerado))
            {
                var erroPadrao = processo.StandardError.ReadToEnd();
                _logger.LogError("Falha ao converter DOCX para PDF. Código de saída: {ExitCode}. Erro: {Erro}",
                    processo.ExitCode, erroPadrao);
                throw new ConversaoPdfIndisponivelException("Não foi possível gerar o PDF do contrato.");
            }

            return File.ReadAllBytes(caminhoPdfGerado);
        }
        finally
        {
            try
            {
                Directory.Delete(pastaTemporaria, recursive: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível remover a pasta temporária {Pasta}.", pastaTemporaria);
            }
        }
    }
}