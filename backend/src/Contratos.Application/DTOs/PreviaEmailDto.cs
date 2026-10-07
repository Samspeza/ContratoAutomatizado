namespace Contratos.Application.DTOs;

public sealed record PreviaEmailDto(
    string Remetente,
    string Destinatario,
    string Assunto,
    string Mensagem,
    string NomeArquivoAnexo);