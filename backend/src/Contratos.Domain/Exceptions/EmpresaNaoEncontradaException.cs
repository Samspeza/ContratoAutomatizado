namespace Contratos.Domain.Exceptions;

public sealed class EmpresaNaoEncontradaException : Exception
{
    public EmpresaNaoEncontradaException()
        : base("Não foi possível localizar o CNPJ informado.") { }
}