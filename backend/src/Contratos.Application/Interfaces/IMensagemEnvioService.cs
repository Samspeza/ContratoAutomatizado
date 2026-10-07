using Contratos.Application.DTOs;
using Contratos.Domain.Entities;

namespace Contratos.Application.Interfaces;

public interface IMensagemEnvioService
{
    PreviaEmailDto MontarPreviaEmail(Contrato contrato);
    PreviaWhatsappDto MontarPreviaWhatsapp(Contrato contrato);
}