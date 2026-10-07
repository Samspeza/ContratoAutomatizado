namespace Contratos.Domain.Exceptions;

public sealed class DestinatariosInvalidosException : Exception
{
    public IReadOnlyList<string> Mensagens { get; }

    public DestinatariosInvalidosException(IReadOnlyList<string> mensagens)
        : base("Destinatários inválidos.")
    {
        Mensagens = mensagens;
    }
}