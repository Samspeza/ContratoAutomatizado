using Contratos.Application.DTOs;

namespace Contratos.Application.Interfaces;

public interface IGeradorDocumentoService
{
    byte[] GerarContrato(DadosContratoDto dados);
}