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
    private readonly string _host;
    private readonly int _porta;
    private readonly string _usuario;
    private readonly string _senha;
    private readonly string _remetente;
    private readonly IMensagemEnvioService _mensagens;
    private readonly ILogger<SmtpEmailEnvioService> _logger;

    public SmtpEmailEnvioService(
        IConfiguration configuration,
        IMensagemEnvioService mensagens,
        ILogger<SmtpEmailEnvioService> logger)
    {
        _host = configuration["Envio:Email:Smtp:Host"]
            ?? throw new InvalidOperationException("Configuração 'Envio:Email:Smtp:Host' não encontrada.");
        _porta = configuration.GetValue<int?>("Envio:Email:Smtp:Porta")
            ?? throw new InvalidOperationException("Configuração 'Envio:Email:Smtp:Porta' não encontrada.");
        _usuario = configuration["Envio:Email:Smtp:Usuario"]
            ?? throw new InvalidOperationException("Configuração 'Envio:Email:Smtp:Usuario' não encontrada.");
        _senha = configuration["Envio:Email:Smtp:Senha"]
            ?? throw new InvalidOperationException(
                "Configuração 'Envio:Email:Smtp:Senha' não encontrada. Configure-a via User Secrets (desenvolvimento) ou variável de ambiente (produção) — nunca no appsettings.json.");

        _remetente = configuration["Envio:Email:Remetente"]
            ?? throw new InvalidOperationException("Configuração 'Envio:Email:Remetente' não encontrada.");

        _mensagens = mensagens;
        _logger = logger;
    }

    public async Task EnviarAsync(Contrato contrato)
    {
        if (string.IsNullOrWhiteSpace(contrato.CaminhoArquivoPdf) || !File.Exists(contrato.CaminhoArquivoPdf))
            throw new EnvioEmailFalhouException("O PDF deste contrato não foi encontrado para anexar ao e-mail.");

        var previa = _mensagens.MontarPreviaEmail(contrato);

        var mensagem = new MimeMessage();
        mensagem.From.Add(MailboxAddress.Parse(_remetente));
        mensagem.To.Add(MailboxAddress.Parse(previa.Destinatario));
        mensagem.Subject = previa.Assunto;

        var corpo = new BodyBuilder
        {
            TextBody = previa.Mensagem
        };

        var pdfBytes = File.ReadAllBytes(contrato.CaminhoArquivoPdf);

        corpo.Attachments.Add(
            previa.NomeArquivoAnexo,
            pdfBytes,
            new ContentType("application", "pdf")
        );

        mensagem.Body = corpo.ToMessageBody();

        using var cliente = new SmtpClient();

        try
        {
            await cliente.ConnectAsync(_host, _porta, SecureSocketOptions.StartTls);
            await cliente.AuthenticateAsync(_usuario, _senha);
            await cliente.SendAsync(mensagem);
            await cliente.DisconnectAsync(quit: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar e-mail do contrato {ContratoId} via SMTP ({Host}:{Porta}).", contrato.Id, _host, _porta);
            throw new EnvioEmailFalhouException(
                "Não foi possível enviar o e-mail. Verifique o destinatário e tente novamente, ou consulte os registros em Documentos\\ContratoAutomatizado\\Logs.",
                ex);
        }
    }
}