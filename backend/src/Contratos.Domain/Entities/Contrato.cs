using Contratos.Domain.Enums;

namespace Contratos.Domain.Entities;

public sealed class Contrato
{
    public int Id { get; set; }

    // Snapshot dos dados da empresa no momento da consulta — não referenciamos uma tabela de empresas.
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;

    public decimal PercentualHonorarios { get; set; }
    public int QuantidadeParcelas { get; set; }
    public DateOnly DataContrato { get; set; }

    public string? CaminhoArquivoDocx { get; set; }
    public string? CaminhoArquivoPdf { get; set; }

    // Guarda os dados que originaram este contrato, para permitir gerar novamente
    // sem redigitar nada caso a geração falhe (LibreOffice indisponível, etc.).
    public string DadosOriginaisJson { get; set; } = string.Empty;

    public StatusContrato Status { get; set; }

    public string? EmailDestinatario { get; set; }
    public StatusCanalEnvio StatusEmail { get; set; }
    public DateTime? EmailEnviadoEm { get; set; }
    public string? EmailErroMensagem { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public List<ContratoResponsavel> Responsaveis { get; set; } = new();
    public List<EventoHistorico> Historico { get; set; } = new();

    public bool PodeGerarArquivos =>
        Status is StatusContrato.Rascunho or StatusContrato.ErroGeracao;

    public bool PodeEnviarEmail =>
        Status is StatusContrato.ProntoParaEnvio or StatusContrato.ErroNoEnvio or StatusContrato.Concluido;

    public bool PodeCancelar =>
        Status is not (StatusContrato.Concluido or StatusContrato.Cancelado);

    public static Contrato CriarRascunho(
        string cnpj,
        string razaoSocial,
        string endereco,
        decimal percentualHonorarios,
        int quantidadeParcelas,
        DateOnly dataContrato,
        IEnumerable<ContratoResponsavel> responsaveis)
    {
        var agora = DateTime.UtcNow;

        var contrato = new Contrato
        {
            Cnpj = cnpj,
            RazaoSocial = razaoSocial,
            Endereco = endereco,
            PercentualHonorarios = percentualHonorarios,
            QuantidadeParcelas = quantidadeParcelas,
            DataContrato = dataContrato,
            Status = StatusContrato.Rascunho,
            StatusEmail = StatusCanalEnvio.Pendente,
            CriadoEm = agora,
            AtualizadoEm = agora
        };

        contrato.Responsaveis.AddRange(responsaveis);
        contrato.Historico.Add(new EventoHistorico
        {
            DataHora = agora,
            Tipo = TipoEventoHistorico.ContratoCriado,
            Descricao = "Contrato criado a partir dos dados informados."
        });

        return contrato;
    }

    public void MarcarComoGerado(string caminhoDocx, string caminhoPdf)
    {
        CaminhoArquivoDocx = caminhoDocx;
        CaminhoArquivoPdf = caminhoPdf;
        Status = StatusContrato.Gerado;
        Historico.Add(new EventoHistorico
        {
            DataHora = DateTime.UtcNow,
            Tipo = TipoEventoHistorico.ContratoGerado,
            Descricao = "Contrato gerado (DOCX e PDF)."
        });
    }

    public void MarcarErroNaGeracao(string motivo)
    {
        Status = StatusContrato.ErroGeracao;
        Historico.Add(new EventoHistorico
        {
            DataHora = DateTime.UtcNow,
            Tipo = TipoEventoHistorico.FalhaNaGeracao,
            Descricao = motivo
        });
    }

    // Idempotente: se o contrato já está na etapa de envio (ou além), não faz nada.
    // Assim o usuário pode voltar ao documento e continuar de novo sem erro e sem eventos duplicados.
    public void AvancarParaEnvio()
    {
        if (PodeEnviarEmail) return;

        if (Status != StatusContrato.Gerado)
            throw new InvalidOperationException("Só é possível avançar para o envio um contrato que já foi gerado.");

        Status = StatusContrato.ProntoParaEnvio;
        Historico.Add(new EventoHistorico
        {
            DataHora = DateTime.UtcNow,
            Tipo = TipoEventoHistorico.AvancouParaEnvio,
            Descricao = "Usuário confirmou o documento e avançou para a etapa de envio."
        });
    }

    public void DefinirEmailDestinatario(string? email)
    {
        EmailDestinatario = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
    }

    public void MarcarEmailEnviado()
    {
        StatusEmail = StatusCanalEnvio.Enviado;
        EmailEnviadoEm = DateTime.UtcNow;
        EmailErroMensagem = null;
        Status = StatusContrato.Concluido;
        Historico.Add(new EventoHistorico
        {
            DataHora = DateTime.UtcNow,
            Tipo = TipoEventoHistorico.EmailEnviado,
            Descricao = $"E-mail enviado para {EmailDestinatario}."
        });
    }

    // Mantém EmailEnviadoEm: se um reenvio falhar, ainda dá para ver quando houve o último envio com sucesso.
    public void MarcarErroEnvioEmail(string motivo)
    {
        StatusEmail = StatusCanalEnvio.Erro;
        EmailErroMensagem = motivo;
        Status = StatusContrato.ErroNoEnvio;
        Historico.Add(new EventoHistorico
        {
            DataHora = DateTime.UtcNow,
            Tipo = TipoEventoHistorico.FalhaEnvioEmail,
            Descricao = motivo
        });
    }

    public void Cancelar()
    {
        if (!PodeCancelar)
            throw new InvalidOperationException("Este contrato não pode mais ser cancelado.");

        Status = StatusContrato.Cancelado;
        Historico.Add(new EventoHistorico
        {
            DataHora = DateTime.UtcNow,
            Tipo = TipoEventoHistorico.ContratoCancelado,
            Descricao = "Contrato cancelado pelo usuário."
        });
    }
}