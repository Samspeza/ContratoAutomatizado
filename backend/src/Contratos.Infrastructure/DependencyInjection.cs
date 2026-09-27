using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Cnpj;
using Contratos.Infrastructure.Documents;
using Contratos.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            client.Timeout = TimeSpan.FromSeconds(10);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "ContratoAutomatizado/1.0");
        });

        services.AddSingleton<IGeradorDocumentoService, OpenXmlGeradorDocumentoService>();
        services.AddSingleton<IArmazenamentoContratoService, ArmazenamentoContratoLocalService>();
        services.AddSingleton<IPdfConversorService, LibreOfficePdfConversorService>();

        return services;
    }
}