using Contratos.Application.DTOs;
using Contratos.Application.Formatting;
using Contratos.Application.Interfaces;
using Contratos.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Envio;

// ATENÇÃO: as mensagens aqui são provisórias, só para validar a estrutura da prévia
// (remetente, destinatário, assunto, anexo). O texto real será fornecido pelo usuário
// e passa a ser configurável nas Etapas 7 (e-mail) e 9 (WhatsApp) — este serviço será
// substituído por um que lê a mensagem de configuração, não mais um texto fixo aqui.
public sealed class MensagemEnvioProvisoriaService : IMensagemEnvioService
{
    private readonly string _remetenteEmail;

    public MensagemEnvioProvisoriaService(IConfiguration configuration)
    {
        _remetenteEmail = configuration["Envio:Email:Remetente"]
            ?? throw new InvalidOperationException("Configuração 'Envio:Email:Remetente' não encontrada.");
    }

    public PreviaEmailDto MontarPreviaEmail(Contrato contrato)
    {
        if (string.IsNullOrWhiteSpace(contrato.EmailDestinatario))
            throw new InvalidOperationException("Este contrato não possui um destinatário de e-mail configurado.");

        var assunto = $"[Mensagem provisória] Contrato — {contrato.RazaoSocial}";
        var mensagem =
            "[Este é um texto provisório, apenas para validar a estrutura do envio. " +
            "A mensagem definitiva será configurada.]\n\n" +
            $"Empresa: {contrato.RazaoSocial}\n" +
            $"CNPJ: {CnpjFormatter.Formatar(contrato.Cnpj)}";

        return new PreviaEmailDto(
            _remetenteEmail,
            contrato.EmailDestinatario,
            assunto,
            mensagem,
            Path.GetFileName(contrato.CaminhoArquivoPdf) ?? string.Empty);
    }

    public PreviaWhatsappDto MontarPreviaWhatsapp(Contrato contrato)
    {
        if (string.IsNullOrWhiteSpace(contrato.WhatsappDestinatario))
            throw new InvalidOperationException("Este contrato não possui um número de WhatsApp configurado.");

        var mensagem =
            "[Texto provisório — a mensagem definitiva será configurada.]\n\n" +
            $"Segue o contrato da empresa {contrato.RazaoSocial} em anexo.";

        return new PreviaWhatsappDto(
            contrato.WhatsappDestinatario,
            mensagem,
            Path.GetFileName(contrato.CaminhoArquivoPdf) ?? string.Empty);
    }
}