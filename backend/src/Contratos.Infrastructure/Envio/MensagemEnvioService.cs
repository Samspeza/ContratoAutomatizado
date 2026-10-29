using Contratos.Application.DTOs;
using Contratos.Application.Interfaces;
using Contratos.Application.Messages;
using Contratos.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Envio;

public sealed class MensagemEnvioService : IMensagemEnvioService
{
    private readonly string _remetenteEmail;
    private readonly IModeloMensagemProvider _modelos;

    public MensagemEnvioService(IConfiguration configuration, IModeloMensagemProvider modelos)
    {
        _remetenteEmail = configuration["Envio:Email:Remetente"]
            ?? throw new InvalidOperationException("Configuração 'Envio:Email:Remetente' não encontrada.");
        _modelos = modelos;
    }

    public PreviaEmailDto MontarPreviaEmail(Contrato contrato)
    {
        if (string.IsNullOrWhiteSpace(contrato.EmailDestinatario))
            throw new InvalidOperationException("Este contrato não possui um destinatário de e-mail configurado.");

        var modelo = _modelos.ObterModeloEmail();
        var valores = PlaceholdersMensagem.Montar(contrato);

        return new PreviaEmailDto(
            _remetenteEmail,
            contrato.EmailDestinatario,
            PlaceholdersMensagem.Substituir(modelo.Assunto, valores),
            PlaceholdersMensagem.Substituir(modelo.Corpo, valores),
            Path.GetFileName(contrato.CaminhoArquivoPdf) ?? string.Empty);
    }
}