namespace Contratos.Domain.Exceptions;

public sealed class EnvioEmailFalhouException : Exception
{
    public EnvioEmailFalhouException(string mensagem, Exception? erroInterno = null)
        : base(mensagem, erroInterno) { }
}