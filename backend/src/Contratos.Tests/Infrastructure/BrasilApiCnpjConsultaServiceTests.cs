using System.Net;
using Contratos.Domain.Exceptions;
using Contratos.Domain.ValueObjects;
using Contratos.Infrastructure.Cnpj;
using Xunit;

namespace Contratos.Tests.Infrastructure;

public class BrasilApiCnpjConsultaServiceTests
{
    private sealed class RespostaFalsaHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string? _conteudo;
        private readonly bool _lancarExcecaoDeRede;

        public RespostaFalsaHttpMessageHandler(HttpStatusCode statusCode, string? conteudo = null, bool lancarExcecaoDeRede = false)
        {
            _statusCode = statusCode;
            _conteudo = conteudo;
            _lancarExcecaoDeRede = lancarExcecaoDeRede;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_lancarExcecaoDeRede)
                throw new HttpRequestException("Falha simulada de rede.");

            var resposta = new HttpResponseMessage(_statusCode)
            {
                Content = _conteudo is not null ? new StringContent(_conteudo) : null
            };
            return Task.FromResult(resposta);
        }
    }

    private static BrasilApiCnpjConsultaService CriarServico(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://fake.brasilapi.com.br/") };
        return new BrasilApiCnpjConsultaService(httpClient);
    }

    [Fact]
    public async Task ConsultarAsync_ComRespostaValida_DeveRetornarEmpresaPreenchida()
    {
        const string json = """
        {
          "cnpj": "00000000000191",
          "razao_social": "EMPRESA TESTE LTDA",
          "logradouro": "Rua Teste",
          "numero": "100",
          "bairro": "Centro",
          "municipio": "São Paulo",
          "uf": "SP",
          "cep": "01000000"
        }
        """;
        var servico = CriarServico(new RespostaFalsaHttpMessageHandler(HttpStatusCode.OK, json));

        var empresa = await servico.ConsultarAsync(Cnpj.Criar("00000000000191"));

        Assert.Equal("EMPRESA TESTE LTDA", empresa.RazaoSocial);
        Assert.Equal("São Paulo", empresa.Municipio);
    }

    [Fact]
    public async Task ConsultarAsync_ComCnpjInexistente_DeveLancarEmpresaNaoEncontradaException()
    {
        var servico = CriarServico(new RespostaFalsaHttpMessageHandler(HttpStatusCode.NotFound));

        await Assert.ThrowsAsync<EmpresaNaoEncontradaException>(
            () => servico.ConsultarAsync(Cnpj.Criar("00000000000191")));
    }

    [Fact]
    public async Task ConsultarAsync_ComApiIndisponivel_DeveLancarConsultaCnpjIndisponivelException()
    {
        var servico = CriarServico(new RespostaFalsaHttpMessageHandler(HttpStatusCode.ServiceUnavailable));

        await Assert.ThrowsAsync<ConsultaCnpjIndisponivelException>(
            () => servico.ConsultarAsync(Cnpj.Criar("00000000000191")));
    }

    [Fact]
    public async Task ConsultarAsync_ComFalhaDeRede_DeveLancarConsultaCnpjIndisponivelException()
    {
        var servico = CriarServico(new RespostaFalsaHttpMessageHandler(HttpStatusCode.OK, lancarExcecaoDeRede: true));

        await Assert.ThrowsAsync<ConsultaCnpjIndisponivelException>(
            () => servico.ConsultarAsync(Cnpj.Criar("00000000000191")));
    }
}