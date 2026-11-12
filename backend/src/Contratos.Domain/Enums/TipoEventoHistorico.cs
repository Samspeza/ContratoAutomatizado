namespace Contratos.Domain.Enums;

public enum TipoEventoHistorico
{
    ContratoCriado,
    ContratoGerado,
    FalhaNaGeracao,
    AvancouParaEnvio,
    EmailEnviado,
    FalhaEnvioEmail,
    ContratoCancelado
}