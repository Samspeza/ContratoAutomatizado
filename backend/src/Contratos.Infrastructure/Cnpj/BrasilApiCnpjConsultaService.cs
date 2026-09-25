using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Contratos.Application.Interfaces;
using Contratos.Domain.Entities;
using Contratos.Domain.Exceptions;
using Contratos.Domain.ValueObjects;
using CnpjValueObject = Contratos.Domain.ValueObjects.Cnpj;

namespace Contratos.Infrastructure.Cnpj;

public sealed class BrasilApiCnpjConsultaService : ICnpjConsultaService
{
    private readonly HttpClient _httpClient;

    public BrasilApiCnpjConsultaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Empresa> ConsultarAsync(CnpjValueObject cnpj)
    {
        HttpResponseMessage resposta;

        try
        {
            resposta = await _httpClient.GetAsync(cnpj.Numero);
        }
        catch (TaskCanceledException)
        {
            throw new ConsultaCnpjIndisponivelException();
        }
        catch (HttpRequestException)
        {
            throw new ConsultaCnpjIndisponivelException();
        }

        if (resposta.StatusCode == HttpStatusCode.NotFound)
            throw new EmpresaNaoEncontradaException();

        if (!resposta.IsSuccessStatusCode)
            throw new ConsultaCnpjIndisponivelException();

        var conteudo = await resposta.Content.ReadAsStringAsync();
        var dados = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(conteudo)
            ?? throw new ConsultaCnpjIndisponivelException();

        return new Empresa
        {
            Cnpj = dados.Cnpj ?? cnpj.Numero,
            RazaoSocial = dados.RazaoSocial ?? string.Empty,
            NomeFantasia = dados.NomeFantasia,
            Logradouro = dados.Logradouro ?? string.Empty,
            Numero = dados.Numero,
            Complemento = dados.Complemento,
            Bairro = dados.Bairro ?? string.Empty,
            Municipio = dados.Municipio ?? string.Empty,
            Uf = dados.Uf ?? string.Empty,
            Cep = dados.Cep ?? string.Empty,
            SituacaoCadastral = dados.DescricaoSituacaoCadastral
        };
    }

    private sealed class BrasilApiCnpjResponse
    {
        [JsonPropertyName("cnpj")] public string? Cnpj { get; set; }
        [JsonPropertyName("razao_social")] public string? RazaoSocial { get; set; }
        [JsonPropertyName("nome_fantasia")] public string? NomeFantasia { get; set; }
        [JsonPropertyName("descricao_situacao_cadastral")] public string? DescricaoSituacaoCadastral { get; set; }
        [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
        [JsonPropertyName("numero")] public string? Numero { get; set; }
        [JsonPropertyName("complemento")] public string? Complemento { get; set; }
        [JsonPropertyName("bairro")] public string? Bairro { get; set; }
        [JsonPropertyName("municipio")] public string? Municipio { get; set; }
        [JsonPropertyName("uf")] public string? Uf { get; set; }
        [JsonPropertyName("cep")] public string? Cep { get; set; }
    }
}