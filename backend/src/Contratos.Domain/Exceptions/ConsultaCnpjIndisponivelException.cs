namespace Contratos.Domain.Exceptions;

public sealed class ConsultaCnpjIndisponivelException : Exception
{
    public ConsultaCnpjIndisponivelException()
        : base("Não foi possível consultar os dados do CNPJ neste momento.") { }
}