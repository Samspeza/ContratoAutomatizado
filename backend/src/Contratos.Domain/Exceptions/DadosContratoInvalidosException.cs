namespace Contratos.Domain.Exceptions;

public sealed class DadosContratoInvalidosException : Exception
{
    public IReadOnlyList<string> Mensagens { get; }

    public DadosContratoInvalidosException(IReadOnlyList<string> mensagens)
        : base("Dados do contrato inválidos.")
    {
        Mensagens = mensagens;
    }
}