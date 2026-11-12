using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Cnpj;
using Contratos.Infrastructure.Configuracoes;
using Contratos.Infrastructure.Documents;
using Contratos.Infrastructure.Envio;
using Contratos.Infrastructure.Persistence;
using Contratos.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contratos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("O Contrato Automatizado funciona somente no Windows.");

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
        services.AddHttpClient<ICnpjConsultaService, BrasilApiCnpjConsultaService>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddSingleton<IGeradorDocumentoService, OpenXmlGeradorDocumentoService>();
        services.AddSingleton<IArmazenamentoContratoService, ArmazenamentoContratoLocalService>();
        services.AddSingleton<IPdfConversorService, LibreOfficePdfConversorService>();

        services.AddSingleton<IConfiguracoesUsuarioService, ConfiguracoesUsuarioService>();
        services.AddSingleton<ISegredosService, SegredosDpapiService>();
        services.AddSingleton<IVerificacaoSistemaService, VerificacaoSistemaService>();

        services.AddSingleton<IModeloMensagemProvider, ModeloMensagemArquivoProvider>();
        services.AddSingleton<IMensagemEnvioService, MensagemEnvioService>();
        services.AddSingleton<IEmailEnvioService, SmtpEmailEnvioService>();

        var pastas = new PastasAplicacao(configuration);
        var caminhoBanco = Path.Combine(pastas.Dados, "contratos.db");

        services.AddDbContext<ContratosDbContext>(opcoes =>
            opcoes.UseSqlite($"Data Source={caminhoBanco}"));

        services.AddScoped<IContratoRepository, ContratoRepository>();

        return services;
    }
}