
using Contratos.Application.DTOs;
using Contratos.Application.Validation;
using Contratos.Domain.Exceptions;
using Xunit;

namespace Contratos.Tests.Application;

public class DadosContratoValidatorTests
{
    private static EmpresaDto CriarEmpresaValida() => new(
        "12345678000190",
        "EMPRESA TESTE LTDA",
        null,
        "Rua Teste",
        "100",
        null,
        "Centro",
        "São Paulo",
        "SP",
        "01000-000",
        null);

    private static DadosContratoDto CriarDadosValidos(
        decimal? percentual = null,
        int? parcelas = null,
        DateOnly? data = null,
        List<ResponsavelAssinaturaDto>? responsaveis = null)
    {
        return new DadosContratoDto(
            Empresa: CriarEmpresaValida(),
            ComplementoEndereco: null,
            PercentualHonorarios: percentual ?? 20,
            QuantidadeParcelas: parcelas ?? 3,
            DataContrato: data,
            Responsaveis: responsaveis ?? new List<ResponsavelAssinaturaDto>
            {
                new("Nome Teste", "111.444.777-35")
            });
    }

    [Fact]
    public void Validar_ComDadosValidos_NaoDeveLancarExcecao()
    {
        var dados = CriarDadosValidos(
            data: new DateOnly(2026, 9, 25));

        var excecao = Record.Exception(() => DadosContratoValidator.Validar(dados));

        Assert.Null(excecao);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(150)]
    public void Validar_ComPercentualForaDaFaixa_DeveLancarExcecao(decimal percentualInvalido)
    {
        var dados = CriarDadosValidos(
            percentual: percentualInvalido,
            data: new DateOnly(2026, 9, 25));

        var excecao = Assert.Throws<DadosContratoInvalidosException>(
            () => DadosContratoValidator.Validar(dados));

        Assert.Contains(excecao.Mensagens, m => m.Contains("honorários"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Validar_ComParcelasForaDaFaixa_DeveLancarExcecao(int parcelasInvalidas)
    {
        var dados = CriarDadosValidos(
            parcelas: parcelasInvalidas,
            data: new DateOnly(2026, 9, 25));

        var excecao = Assert.Throws<DadosContratoInvalidosException>(
            () => DadosContratoValidator.Validar(dados));

        Assert.Contains(excecao.Mensagens, m => m.Contains("parcelas"));
    }

    [Fact]
    public void Validar_SemResponsaveis_DeveLancarExcecao()
    {
        var dados = CriarDadosValidos(
            data: new DateOnly(2026, 9, 25),
            responsaveis: new List<ResponsavelAssinaturaDto>());

        var excecao = Assert.Throws<DadosContratoInvalidosException>(
            () => DadosContratoValidator.Validar(dados));

        Assert.Contains(excecao.Mensagens, m => m.Contains("responsável"));
    }

    [Fact]
    public void Validar_ComMaisDeDoisResponsaveis_DeveLancarExcecao()
    {
        var responsaveis = new List<ResponsavelAssinaturaDto>
        {
            new("Um", "111.444.777-35"),
            new("Dois", "111.444.777-35"),
            new("Três", "111.444.777-35")
        };

        var dados = CriarDadosValidos(
            data: new DateOnly(2026, 9, 25),
            responsaveis: responsaveis);

        var excecao = Assert.Throws<DadosContratoInvalidosException>(
            () => DadosContratoValidator.Validar(dados));

        Assert.Contains(excecao.Mensagens, m => m.Contains("no máximo"));
    }

    [Fact]
    public void Validar_ComCpfInvalidoDoResponsavel_DeveLancarExcecao()
    {
        var dados = CriarDadosValidos(
            data: new DateOnly(2026, 9, 25),
            responsaveis: new List<ResponsavelAssinaturaDto>
            {
                new("Nome Teste", "111.111.111-11")
            });

        var excecao = Assert.Throws<DadosContratoInvalidosException>(
            () => DadosContratoValidator.Validar(dados));

        Assert.Contains(excecao.Mensagens, m => m.Contains("CPF inválido"));
    }

    [Fact]
    public void Validar_ComDataNaoInformada_DeveLancarExcecao()
    {
        var dados = CriarDadosValidos(data: null);

        var excecao = Assert.Throws<DadosContratoInvalidosException>(
            () => DadosContratoValidator.Validar(dados));

        Assert.Contains(excecao.Mensagens, m => m.Contains("data"));
    }

    [Fact]
    public void Validar_ComCpfNaoInformado_NaoDeveLancarExcecao()
    {
        var dados = CriarDadosValidos(responsaveis: new List<ResponsavelAssinaturaDto> { new("Nome Teste", "") });
        var excecao = Record.Exception(() => DadosContratoValidator.Validar(dados));
        Assert.Null(excecao);
    }
}

