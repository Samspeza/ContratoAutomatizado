using System.Text.Json;
using Contratos.Application.DTOs;
using Contratos.Application.Formatting;
using Contratos.Application.Interfaces;
using Contratos.Application.Validation;
using Contratos.Domain.Entities;
using Contratos.Domain.Exceptions;

namespace Contratos.Api.Endpoints;

public static class ContratoEndpoints
{
    public static void MapContratoEndpoints(this WebApplication app)
    {
        app.MapPost("/api/contratos", CriarEGerarAsync);
        app.MapPost("/api/contratos/{id:int}/gerar", GerarNovamenteAsync);
        app.MapGet("/api/contratos/{id:int}", ObterAsync);
        app.MapGet("/api/contratos/{id:int}/pdf", (int id, bool? baixar, IContratoRepository repositorio) =>
            ObterArquivoAsync(id, ".pdf", baixar ?? false, repositorio));
        app.MapGet("/api/contratos/{id:int}/docx", (int id, IContratoRepository repositorio) =>
            ObterArquivoAsync(id, ".docx", true, repositorio));
    }

    private static async Task<IResult> CriarEGerarAsync(
        DadosContratoDto dados,
        IContratoRepository repositorio,
        IGeradorDocumentoService gerador,
        IPdfConversorService conversorPdf,
        IArmazenamentoContratoService armazenamento,
        ILogger<Program> logger)
    {
        try
        {
            DadosContratoValidator.Validar(dados);
        }
        catch (DadosContratoInvalidosException ex)
        {
            return Results.BadRequest(new { mensagens = ex.Mensagens });
        }

        var contrato = Contrato.CriarRascunho(
            dados.Empresa.Cnpj,
            dados.Empresa.RazaoSocial,
            EnderecoFormatter.Formatar(dados.Empresa),
            dados.PercentualHonorarios,
            dados.QuantidadeParcelas,
            dados.DataContrato.Value,
            dados.Responsaveis.Select(r => new ContratoResponsavel
            {
                Nome = r.Nome,
                Cpf = string.IsNullOrWhiteSpace(r.Cpf) ? null : r.Cpf
            }));

        contrato.DadosOriginaisJson = JsonSerializer.Serialize(dados);

        await repositorio.CriarAsync(contrato);

        return await GerarArquivosAsync(contrato, dados, gerador, conversorPdf, armazenamento, repositorio, logger);
    }

    private static async Task<IResult> GerarNovamenteAsync(
        int id,
        IContratoRepository repositorio,
        IGeradorDocumentoService gerador,
        IPdfConversorService conversorPdf,
        IArmazenamentoContratoService armazenamento,
        ILogger<Program> logger)
    {
        var contrato = await repositorio.ObterPorIdAsync(id);
        if (contrato is null) return Results.NotFound();

        if (string.IsNullOrWhiteSpace(contrato.DadosOriginaisJson))
        {
            return Results.Problem(
                "Este contrato não possui os dados originais para gerar novamente.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var dados = JsonSerializer.Deserialize<DadosContratoDto>(contrato.DadosOriginaisJson)
            ?? throw new InvalidOperationException("Dados originais do contrato corrompidos.");

        return await GerarArquivosAsync(contrato, dados, gerador, conversorPdf, armazenamento, repositorio, logger);
    }

    private static async Task<IResult> GerarArquivosAsync(
        Contrato contrato,
        DadosContratoDto dados,
        IGeradorDocumentoService gerador,
        IPdfConversorService conversorPdf,
        IArmazenamentoContratoService armazenamento,
        IContratoRepository repositorio,
        ILogger<Program> logger)
    {
        try
        {
            var conteudoDocx = gerador.GerarContrato(dados);

            var nomeBase = armazenamento.DeterminarNomeBaseDisponivel(dados.Empresa.Cnpj);
            var caminhoDocx = armazenamento.SalvarArquivo(conteudoDocx, nomeBase, ".docx");

            var conteudoPdf = conversorPdf.ConverterDocxParaPdf(conteudoDocx);
            var caminhoPdf = armazenamento.SalvarArquivo(conteudoPdf, nomeBase, ".pdf");

            contrato.MarcarComoGerado(caminhoDocx, caminhoPdf);
            await repositorio.AtualizarAsync(contrato);

            return Results.Ok(ContratoDto.DeContrato(contrato));
        }
        catch (Exception ex) when (ex is FileNotFoundException or ConversaoPdfIndisponivelException)
        {
            contrato.MarcarErroNaGeracao(ex.Message);
            await repositorio.AtualizarAsync(contrato);

            logger.LogError(ex, "Falha ao gerar o contrato {ContratoId}.", contrato.Id);
            return Results.Json(
                new { contratoId = contrato.Id, mensagem = "Não foi possível gerar o contrato. Você pode tentar novamente sem perder os dados." },
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static async Task<IResult> ObterAsync(int id, IContratoRepository repositorio)
    {
        var contrato = await repositorio.ObterPorIdAsync(id);
        return contrato is null ? Results.NotFound() : Results.Ok(ContratoDto.DeContrato(contrato));
    }

    private static async Task<IResult> ObterArquivoAsync(int id, string extensao, bool baixar, IContratoRepository repositorio)
    {
        var contrato = await repositorio.ObterPorIdAsync(id);
        if (contrato is null) return Results.NotFound();

        var caminho = extensao == ".pdf" ? contrato.CaminhoArquivoPdf : contrato.CaminhoArquivoDocx;
        if (string.IsNullOrWhiteSpace(caminho) || !File.Exists(caminho))
            return Results.NotFound(new { mensagem = "O arquivo deste contrato ainda não foi gerado." });

        var bytes = await File.ReadAllBytesAsync(caminho);
        var tipoConteudo = extensao == ".pdf"
            ? "application/pdf"
            : "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        var nomeArquivo = baixar ? Path.GetFileName(caminho) : null;
        return Results.File(bytes, tipoConteudo, nomeArquivo, enableRangeProcessing: true);
    }
}