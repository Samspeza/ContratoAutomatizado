using Contratos.Application.DTOs;

namespace Contratos.Application.Interfaces;

public interface IVerificacaoSistemaService
{
    IReadOnlyList<ItemVerificacaoDto> Verificar();
}