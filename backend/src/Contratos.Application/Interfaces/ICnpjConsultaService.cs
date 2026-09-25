using Contratos.Domain.Entities;
using Contratos.Domain.ValueObjects;

namespace Contratos.Application.Interfaces;

public interface ICnpjConsultaService
{
    Task<Empresa> ConsultarAsync(Cnpj cnpj);
}