using System.Threading.RateLimiting;
using Contratos.Api.Endpoints;
using Contratos.Api.Middleware;
using Contratos.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var permiteConsultasPorJanela = builder.Configuration.GetValue<int?>("RateLimiting:ConsultaCnpj:PermiteConsultas") ?? 30;
var janelaEmSegundos = builder.Configuration.GetValue<int?>("RateLimiting:ConsultaCnpj:JanelaSegundos") ?? 60;

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ConsultaCnpj", limiterOptions =>
    {
        limiterOptions.PermitLimit = permiteConsultasPorJanela;
        limiterOptions.Window = TimeSpan.FromSeconds(janelaEmSegundos);
        limiterOptions.QueueLimit = 0;
    });

    options.OnRejected = async (contexto, cancellationToken) =>
    {
        contexto.HttpContext.Response.ContentType = "application/json";
        contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await contexto.HttpContext.Response.WriteAsync(
            """{"mensagem":"Muitas consultas em um curto período. Aguarde um instante e tente novamente."}""",
            cancellationToken);
    };
});

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<TratamentoGlobalDeErrosMiddleware>();

app.UseCors("Frontend");
app.UseRateLimiter();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));
app.MapCnpjEndpoints();
app.MapContratoEndpoints();

app.Run();

public partial class Program { }