using Contratos.Application.DTOs;
using Contratos.Application.Interfaces;
using Contratos.Application.Validation;
using Contratos.Domain.Exceptions;

namespace Contratos.Api.Endpoints;

public static class ContratoEndpoints
{
    public static void MapContratoEndpoints(this WebApplication app)
    {
        app.MapPost("/api/contratos", (
            DadosContratoDto dados,
            IGeradorDocumentoService gerador,
            IPdfConversorService conversorPdf,
            IArmazenamentoContratoService armazenamento,
            ILogger<Program> logger) =>
        {
            try
            {
                DadosContratoValidator.Validar(dados);

                var conteudoDocx = gerador.GerarContrato(dados);

                var nomeBase = armazenamento.DeterminarNomeBaseDisponivel(dados.Empresa.Cnpj);
                var caminhoDocx = armazenamento.SalvarArquivo(conteudoDocx, nomeBase, ".docx");

                var conteudoPdf = conversorPdf.ConverterDocxParaPdf(conteudoDocx);
                var caminhoPdf = armazenamento.SalvarArquivo(conteudoPdf, nomeBase, ".pdf");

                return Results.Ok(new
                {
                    arquivoDocx = Path.GetFileName(caminhoDocx),
                    arquivoPdf = Path.GetFileName(caminhoPdf),
                    caminhoCompletoDocx = caminhoDocx,
                    caminhoCompletoPdf = caminhoPdf
                });
            }
            catch (DadosContratoInvalidosException ex)
            {
                return Results.BadRequest(new { mensagens = ex.Mensagens });
            }
            catch (FileNotFoundException ex)
            {
                logger.LogError(ex, "Template de contrato não encontrado.");
                return Results.Problem(
                    detail: "O modelo de contrato não foi encontrado. Verifique a configuração do sistema.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (ConversaoPdfIndisponivelException ex)
            {
                logger.LogError(ex, "Falha ao converter contrato para PDF.");
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao gerar o contrato.");
                return Results.Problem(
                    detail: "Não foi possível gerar o contrato neste momento.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        });
    }
}