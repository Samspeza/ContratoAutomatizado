namespace Contratos.Domain.Enums;

public enum TipoEventoHistorico
{
    ContratoCriado,
    ContratoGerado,
    FalhaNaGeracao,
    AvancouParaEnvio,
    EmailEnviado,
    FalhaEnvioEmail,
    WhatsappEnviado,
    FalhaEnvioWhatsapp,
    ContratoCancelado
}