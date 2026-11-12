using Contratos.Application.Messages;

namespace Contratos.Application.Interfaces;

public interface IModeloMensagemProvider
{
    ModeloMensagem ObterModeloEmail();
    ModeloMensagem ObterModeloPadraoEmail();
    void SalvarModeloEmail(ModeloMensagem modelo);
}