using Contratos.Domain.Entities;

namespace Contratos.Application.Interfaces;

public interface IEmailEnvioService
{
    Task EnviarAsync(Contrato contrato);

    // Conecta e autentica no servidor sem enviar nenhuma mensagem.
    Task TestarConexaoAsync();
}