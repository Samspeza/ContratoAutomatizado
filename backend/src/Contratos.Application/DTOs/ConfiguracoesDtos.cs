namespace Contratos.Application.DTOs;

public sealed record ConfiguracaoEmailDto(
    string Remetente,
    string Host,
    int Porta,
    string Usuario,
    bool SenhaConfigurada,
    string OrigemSenha);

public sealed record ConfiguracaoArmazenamentoDto(
    string PastaContratosGerados,
    string PastaPadraoContratos,
    bool PastaPersonalizada,
    string PastaModelos,
    string PastaLogs,
    string PastaDados);

public sealed record ConfiguracaoAcompanhamentoDto(int DiasParado);

public sealed record ConfiguracaoPdfDto(string CaminhoLibreOffice);

public sealed record ConfiguracoesDto(
    ConfiguracaoEmailDto Email,
    ConfiguracaoArmazenamentoDto Armazenamento,
    ConfiguracaoAcompanhamentoDto Acompanhamento,
    ConfiguracaoPdfDto Pdf);

public sealed record PlaceholderDto(string Nome, string Descricao, string Exemplo);

public sealed record MensagemEmailConfiguradaDto(
    string Assunto,
    string Corpo,
    string AssuntoPadrao,
    string CorpoPadrao,
    IReadOnlyList<PlaceholderDto> Placeholders,
    string? Aviso);

public sealed record SalvarMensagemEmailDto(string Assunto, string Corpo);
public sealed record DefinirSenhaEmailDto(string Senha);
public sealed record AtualizarPastaContratosDto(string? Pasta);
public sealed record AtualizarDiasParadoDto(int Dias);
public sealed record AtualizarCaminhoLibreOfficeDto(string? Caminho);
public sealed record AbrirPastaDto(string Pasta);

public sealed record ItemVerificacaoDto(string Chave, string Titulo, bool Ok, string Detalhe);