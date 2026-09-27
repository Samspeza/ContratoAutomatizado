using Contratos.Domain.ValueObjects;
using Xunit;

namespace Contratos.Tests.Domain;

public class CpfTests
{
    [Theory]
    [InlineData("111.444.777-35")]
    [InlineData("11144477735")]
    public void EhValido_ComCpfValido_DeveRetornarTrue(string cpfValido)
    {
        Assert.True(Cpf.EhValido(cpfValido));
    }

    [Theory]
    [InlineData("111.111.111-11")] // todos os dígitos iguais
    [InlineData("123.456.789-00")] // dígitos verificadores incorretos
    [InlineData("123")]
    [InlineData("")]
    public void EhValido_ComCpfInvalido_DeveRetornarFalse(string cpfInvalido)
    {
        Assert.False(Cpf.EhValido(cpfInvalido));
    }
}