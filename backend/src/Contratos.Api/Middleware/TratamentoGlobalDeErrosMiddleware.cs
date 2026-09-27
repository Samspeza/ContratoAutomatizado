using System.Text.Json;

namespace Contratos.Api.Middleware;

public sealed class TratamentoGlobalDeErrosMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoGlobalDeErrosMiddleware> _logger;

    public TratamentoGlobalDeErrosMiddleware(RequestDelegate proximo, ILogger<TratamentoGlobalDeErrosMiddleware> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _proximo(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao processar {Metodo} {Caminho}.", context.Request.Method, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var corpo = JsonSerializer.Serialize(new
            {
                mensagem = "Ocorreu um erro inesperado. Tente novamente em instantes."
            });

            await context.Response.WriteAsync(corpo);
        }
    }
}