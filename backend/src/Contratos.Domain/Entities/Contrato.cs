using Contratos.Domain.Enums;

namespace Contratos.Domain.Entities;

public sealed class Contrato
{
    public int Id { get; set; }

    // Snapshot dos dados da empresa no momento da consulta — não referenciamos uma tabela de empresas.
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;

    // Guarda os dados que originaram este contrato, para permitir gerar novamente
    // sem redigitar nada caso a geração falhe (LibreOffice indisponível, etc.).
    public string DadosOriginaisJson { get; set; } = string.Empty;

    public decimal PercentualHonorarios { get; set; }
    public int QuantidadeParcelas { get; set; }
    public DateOnly DataContrato { get; set; }

    public string? CaminhoArquivoDocx { get; set; }
    public string? CaminhoArquivoPdf { get; set; }

    public StatusContrato Status { get; set; }

    public string? EmailDestinatario { get; set; }
    public StatusCanalEnvio StatusEmail { get; set; }
    public DateTime? EmailEnviadoEm { get; set; }
    public string? EmailErroMensagem { get; set; }

    public string? WhatsappDestinatario { get; set; }
    public StatusCanalEnvio StatusWhatsapp { get; set; }
    public DateTime? WhatsappEnviadoEm { get; set; }
    public string? WhatsappErroMensagem { get; set; }

    public DateTime CriadoEm { get; set; }
    public DateTime AtualizadoEm { get; set; }

    public List<ContratoResponsavel> Responsaveis { get; set; } = new();
    public List<EventoHistorico> Historico { get; set; } = new();

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
            StatusWhatsapp = StatusCanalEnvio.Pendente,
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

    // Recalcula o status geral a partir dos dois canais — nunca é setado diretamente.
    // Ainda não é chamado em nenhum lugar do sistema; vai passar a ser usado
    // a partir das etapas de envio (6 em diante).
    public void RecalcularStatusDeEnvio()
    {
        if (Status is StatusContrato.Cancelado or StatusContrato.Rascunho or StatusContrato.ErroGeracao or StatusContrato.Gerado)
            return;

        var statusRelevantes = new List<StatusCanalEnvio>();
        if (!string.IsNullOrWhiteSpace(EmailDestinatario)) statusRelevantes.Add(StatusEmail);
        if (!string.IsNullOrWhiteSpace(WhatsappDestinatario)) statusRelevantes.Add(StatusWhatsapp);

        if (statusRelevantes.Count == 0 || statusRelevantes.All(s => s == StatusCanalEnvio.Pendente))
        {
            Status = StatusContrato.ProntoParaEnvio;
        }
        else if (statusRelevantes.All(s => s == StatusCanalEnvio.Enviado))
        {
            Status = StatusContrato.Concluido;
        }
        else if (statusRelevantes.Any(s => s == StatusCanalEnvio.Enviado))
        {
            Status = StatusContrato.EnvioParcial;
        }
        else
        {
            Status = StatusContrato.ErroNoEnvio;
        }
    }
}