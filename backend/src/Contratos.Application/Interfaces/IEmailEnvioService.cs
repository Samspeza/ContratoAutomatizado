using Contratos.Domain.Entities;

namespace Contratos.Application.Interfaces;

public interface IEmailEnvioService
{
    Task EnviarAsync(Contrato contrato);
}