namespace Contratos.Application.DTOs;

public sealed record PreviaWhatsappDto(
    string NumeroDestino,
    string Mensagem,
    string NomeArquivoAnexo);