using Contratos.Domain.Exceptions;
using Contratos.Domain.ValueObjects;
using Xunit;

namespace Contratos.Tests.Domain;

public class CnpjTests
{
    [Theory]
    [InlineData("00.000.000/0001-91")]
    [InlineData("00000000000191")]
    public void Criar_ComCnpjValido_DeveRetornarSemLancarExcecao(string cnpjValido)
    {
        var cnpj = Cnpj.Criar(cnpjValido);
        Assert.Equal("00000000000191", cnpj.Numero);
    }

    [Theory]
    [InlineData("11111111111111")] // todos os dígitos iguais
    [InlineData("12345678901234")] // dígitos verificadores incorretos
    [InlineData("123")]            // tamanho incorreto
    [InlineData("")]
    [InlineData(null)]
    public void Criar_ComCnpjInvalido_DeveLancarCnpjInvalidoException(string? cnpjInvalido)
    {
        Assert.Throws<CnpjInvalidoException>(() => Cnpj.Criar(cnpjInvalido!));
    }

    [Fact]
    public void Formatado_DeveRetornarComMascara()
    {
        var cnpj = Cnpj.Criar("00000000000191");
        Assert.Equal("00.000.000/0001-91", cnpj.Formatado);
    }
}