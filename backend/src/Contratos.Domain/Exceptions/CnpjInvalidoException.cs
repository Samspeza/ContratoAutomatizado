namespace Contratos.Domain.Exceptions;

public sealed class CnpjInvalidoException : Exception
{
    public CnpjInvalidoException(string mensagem) : base(mensagem) { }
}