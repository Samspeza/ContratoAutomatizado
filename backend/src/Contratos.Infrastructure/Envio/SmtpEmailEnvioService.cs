using System.Net.Sockets;
using Contratos.Application.Interfaces;
using Contratos.Domain.Entities;
using Contratos.Domain.Exceptions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Contratos.Infrastructure.Envio;

public sealed class SmtpEmailEnvioService : IEmailEnvioService
{
    private const int TimeoutEmMilissegundos = 30_000;

    private readonly IConfiguration _configuration;
    private readonly ISegredosService _segredos;
    private readonly IMensagemEnvioService _mensagens;
    private readonly ILogger<SmtpEmailEnvioService> _logger;

    private sealed record ConfiguracaoSmtp(string Host, int Porta, string Usuario, string Senha, string Remetente);

    public SmtpEmailEnvioService(
        IConfiguration configuration,
        ISegredosService segredos,
        IMensagemEnvioService mensagens,
        ILogger<SmtpEmailEnvioService> logger)
    {
        _configuration = configuration;
        _segredos = segredos;
        _mensagens = mensagens;
        _logger = logger;
    }

    public async Task EnviarAsync(Contrato contrato)
    {
        if (string.IsNullOrWhiteSpace(contrato.CaminhoArquivoPdf) || !File.Exists(contrato.CaminhoArquivoPdf))
            throw new EnvioEmailFalhouException("O PDF deste contrato não foi encontrado para anexar ao e-mail.");

        var configuracao = ObterConfiguracao();
        var previa = _mensagens.MontarPreviaEmail(contrato);

        var mensagem = new MimeMessage();
        mensagem.From.Add(MailboxAddress.Parse(configuracao.Remetente));
        mensagem.To.Add(MailboxAddress.Parse(previa.Destinatario));
        mensagem.Subject = previa.Assunto;

        var corpo = new BodyBuilder
        {
            TextBody = previa.Mensagem
        };

        var arquivoPdf = await File.ReadAllBytesAsync(contrato.CaminhoArquivoPdf);

        corpo.Attachments.Add(
            previa.NomeArquivoAnexo,
            arquivoPdf,
            new ContentType("application", "pdf")
        );

        mensagem.Body = corpo.ToMessageBody();
        
        using var cliente = new SmtpClient { Timeout = TimeoutEmMilissegundos };

        try
        {
            await ConectarEAutenticarAsync(cliente, configuracao);
            await cliente.SendAsync(mensagem);
            await cliente.DisconnectAsync(quit: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail do contrato {ContratoId} via SMTP ({Host}:{Porta}).",
                contrato.Id, configuracao.Host, configuracao.Porta);
            throw new EnvioEmailFalhouException(TraduzirErro(ex), ex);
        }
    }

    public async Task TestarConexaoAsync()
    {
        var configuracao = ObterConfiguracao();

        using var cliente = new SmtpClient { Timeout = TimeoutEmMilissegundos };

        try
        {
            await ConectarEAutenticarAsync(cliente, configuracao);
            await cliente.DisconnectAsync(quit: true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Teste de conexão SMTP falhou ({Host}:{Porta}).", configuracao.Host, configuracao.Porta);
            throw new EnvioEmailFalhouException(TraduzirErro(ex), ex);
        }
    }

    private static async Task ConectarEAutenticarAsync(SmtpClient cliente, ConfiguracaoSmtp configuracao)
    {
        await cliente.ConnectAsync(configuracao.Host, configuracao.Porta, SecureSocketOptions.StartTls);
        await cliente.AuthenticateAsync(configuracao.Usuario, configuracao.Senha);
    }

    private ConfiguracaoSmtp ObterConfiguracao()
    {
        var host = _configuration["Envio:Email:Smtp:Host"];
        var porta = _configuration.GetValue<int?>("Envio:Email:Smtp:Porta");
        var usuario = _configuration["Envio:Email:Smtp:Usuario"];
        var remetente = _configuration["Envio:Email:Remetente"];

        if (string.IsNullOrWhiteSpace(host) || porta is null || string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(remetente))
        {
            throw new EnvioEmailFalhouException(
                "As configurações do servidor de e-mail estão incompletas. Verifique o arquivo de configuração do sistema.");
        }

        var senha = _segredos.ObterSenhaSmtp();
        if (string.IsNullOrWhiteSpace(senha))
        {
            throw new EnvioEmailFalhouException(
                "A senha do e-mail ainda não foi informada. Acesse Configurações > E-mail para informá-la.");
        }

        return new ConfiguracaoSmtp(host, porta.Value, usuario, senha, remetente);
    }

    private static string TraduzirErro(Exception ex) => ex switch
    {
        SslHandshakeException =>
            "Não foi possível estabelecer uma conexão segura com o servidor de e-mail.",
        AuthenticationException =>
            "O servidor de e-mail recusou o usuário ou a senha. Confira a senha em Configurações > E-mail.",
        SocketException or TimeoutException or IOException =>
            "Não foi possível conectar ao servidor de e-mail. Verifique a conexão com a internet e tente novamente.",
        _ =>
            "Não foi possível enviar o e-mail. Verifique o destinatário e tente novamente, ou consulte os registros em Documentos\\ContratoAutomatizado\\Logs."
    };
}