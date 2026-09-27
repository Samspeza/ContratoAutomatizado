namespace Contratos.Domain.Exceptions;

public sealed class ConversaoPdfIndisponivelException : Exception
{
    public ConversaoPdfIndisponivelException(string mensagem, Exception? erroInterno = null)
        : base(mensagem, erroInterno) { }
}