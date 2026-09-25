using Contratos.Application.DTOs;
using Contratos.Application.Interfaces;
using Contratos.Domain.Exceptions;
using Contratos.Domain.ValueObjects;

namespace Contratos.Api.Endpoints;

public static class CnpjEndpoints
{
    public static void MapCnpjEndpoints(this WebApplication app)
    {
        app.MapGet("/api/cnpj", async (string numero, ICnpjConsultaService consultaService) =>
        {
            Cnpj cnpjValido;
            try
            {
                cnpjValido = Cnpj.Criar(numero);
            }
            catch (CnpjInvalidoException ex)
            {
                return Results.BadRequest(new { mensagem = ex.Message });
            }

            try
            {
                var empresa = await consultaService.ConsultarAsync(cnpjValido);
                return Results.Ok(EmpresaDto.DeEmpresa(empresa));
            }
            catch (EmpresaNaoEncontradaException ex)
            {
                return Results.NotFound(new { mensagem = ex.Message });
            }
            catch (ConsultaCnpjIndisponivelException ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });
    }
}