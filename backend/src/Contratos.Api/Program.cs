using System.Diagnostics;
using System.Threading.RateLimiting;
using Contratos.Api.Endpoints;
using Contratos.Api.Middleware;
using Contratos.Infrastructure;
using Contratos.Infrastructure.Storage;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using Serilog.Events;

// A raiz da aplicação é a pasta do executável (e não a "pasta atual" de quem o iniciou),
// para que o wwwroot e o appsettings sejam encontrados mesmo quando aberto por um atalho.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
});

var pastas = new PastasAplicacao(builder.Configuration);
pastas.GarantirEstrutura();

if (string.IsNullOrWhiteSpace(builder.Configuration["Documentos:CaminhoTemplate"]))
    TemplatePadraoInicializador.GarantirTemplate(pastas);

builder.Host.UseSerilog((contexto, servicos, configuracaoLog) => configuracaoLog
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        Path.Combine(pastas.Logs, "log-.txt"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30));

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

// Serve o React já compilado (pasta wwwroot) pelo próprio backend.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }));
app.MapCnpjEndpoints();
app.MapContratoEndpoints();

app.MapFallbackToFile("index.html");

// Quando iniciado pela janela do sistema (--parent-pid <id>), o servidor se encerra
// sozinho se a janela deixar de existir, para não sobrar processo rodando em segundo plano.
if (int.TryParse(builder.Configuration["parent-pid"], out var idProcessoPai))
{
    _ = Task.Run(async () =>
    {
        try
        {
            using var processoPai = Process.GetProcessById(idProcessoPai);
            await processoPai.WaitForExitAsync();
        }
        catch (ArgumentException)
        {
            // o processo pai já não existe
        }

        app.Lifetime.StopApplication();
    });
}

app.Run();

public partial class Program { }