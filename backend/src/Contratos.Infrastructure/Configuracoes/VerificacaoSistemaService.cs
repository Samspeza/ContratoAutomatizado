using Contratos.Application.DTOs;
using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Configuracoes;

public sealed class VerificacaoSistemaService : IVerificacaoSistemaService
{
    private readonly IConfiguration _configuration;
    private readonly ISegredosService _segredos;

    public VerificacaoSistemaService(IConfiguration configuration, ISegredosService segredos)
    {
        _configuration = configuration;
        _segredos = segredos;
    }

    public IReadOnlyList<ItemVerificacaoDto> Verificar() => new[]
    {
        VerificarPastaContratos(),
        VerificarModelo(),
        VerificarPdf(),
        VerificarSenhaEmail()
    };

    private ItemVerificacaoDto VerificarPastaContratos()
    {
        var pasta = new PastasAplicacao(_configuration).ContratosGerados;

        try
        {
            Directory.CreateDirectory(pasta);
            var teste = Path.Combine(pasta, $".verificacao-{Guid.NewGuid():N}");
            File.WriteAllText(teste, "ok");
            File.Delete(teste);

            return new ItemVerificacaoDto("pasta-contratos", "Pasta dos contratos", true, $"Gravação permitida em {pasta}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            return new ItemVerificacaoDto(
                "pasta-contratos", "Pasta dos contratos", false,
                $"Não foi possível gravar em {pasta}. Escolha outra pasta em Armazenamento.");
        }
    }

    private ItemVerificacaoDto VerificarModelo()
    {
        var configurado = _configuration["Documentos:CaminhoTemplate"];
        var caminho = string.IsNullOrWhiteSpace(configurado)
            ? Path.Combine(new PastasAplicacao(_configuration).Templates, TemplatePadraoInicializador.NomeArquivoTemplate)
            : configurado;

        return File.Exists(caminho)
            ? new ItemVerificacaoDto("modelo", "Modelo do contrato (Word)", true, $"Encontrado em {caminho}")
            : new ItemVerificacaoDto("modelo", "Modelo do contrato (Word)", false,
                $"O modelo não foi encontrado em {caminho}. Coloque o arquivo modelo.docx nessa pasta.");
    }

    private ItemVerificacaoDto VerificarPdf()
    {
        var caminho = _configuration["Pdf:CaminhoExecutavelLibreOffice"] ?? string.Empty;

        return File.Exists(caminho)
            ? new ItemVerificacaoDto("pdf", "Conversor de PDF (LibreOffice)", true, $"Encontrado em {caminho}")
            : new ItemVerificacaoDto("pdf", "Conversor de PDF (LibreOffice)", false,
                $"O LibreOffice não foi encontrado em \"{caminho}\". Instale o LibreOffice ou informe o caminho correto em Alertas e PDF.");
    }

    private ItemVerificacaoDto VerificarSenhaEmail()
    {
        return _segredos.ObterOrigemSenhaSmtp() switch
        {
            OrigemSenha.ArmazenamentoSeguro => new ItemVerificacaoDto(
                "email-senha", "Senha do e-mail", true, "Guardada com segurança neste computador."),
            OrigemSenha.Configuracao => new ItemVerificacaoDto(
                "email-senha", "Senha do e-mail", true, "Definida na configuração do sistema."),
            _ => new ItemVerificacaoDto(
                "email-senha", "Senha do e-mail", false, "A senha ainda não foi informada. Informe-a em E-mail.")
        };
    }
}