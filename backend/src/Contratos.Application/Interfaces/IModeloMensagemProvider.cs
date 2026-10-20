using Contratos.Application.Messages;

namespace Contratos.Application.Interfaces;

public interface IModeloMensagemProvider
{
    ModeloMensagem ObterModeloEmail();
}