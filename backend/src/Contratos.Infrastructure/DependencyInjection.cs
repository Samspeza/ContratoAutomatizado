using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Cnpj;
using Contratos.Infrastructure.Documents;
using Contratos.Infrastructure.Persistence;
using Contratos.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Contratos.Infrastructure.Envio;

namespace Contratos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["CnpjApi:BrasilApi:BaseUrl"]
            ?? throw new InvalidOperationException(
                "Configuração 'CnpjApi:BrasilApi:BaseUrl' não encontrada.");

        services.AddHttpClient<ICnpjConsultaService, BrasilApiCnpjConsultaService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "ContratoAutomatizado/1.0");
        });

        services.AddSingleton<IGeradorDocumentoService, OpenXmlGeradorDocumentoService>();
        services.AddSingleton<IArmazenamentoContratoService, ArmazenamentoContratoLocalService>();
        services.AddSingleton<IPdfConversorService, LibreOfficePdfConversorService>();
        services.AddSingleton<IMensagemEnvioService, MensagemEnvioProvisoriaService>();

        var pastas = new PastasAplicacao(configuration);
        var caminhoBanco = Path.Combine(pastas.Dados, "contratos.db");

        services.AddDbContext<ContratosDbContext>(opcoes =>
            opcoes.UseSqlite($"Data Source={caminhoBanco}"));

        services.AddScoped<IContratoRepository, ContratoRepository>();

        return services;
    }
}