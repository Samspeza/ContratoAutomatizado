using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Storage;

public sealed class PastasAplicacao
{
    public string Raiz { get; }
    public string Templates { get; }
    public string ContratosGerados { get; }
    public string Logs { get; }

    public PastasAplicacao(IConfiguration configuration)
    {
        var raizConfigurada = configuration["Armazenamento:PastaRaiz"];

        Raiz = string.IsNullOrWhiteSpace(raizConfigurada)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ContratoAutomatizado")
            : raizConfigurada;

        Templates = Path.Combine(Raiz, "Templates");
        Logs = Path.Combine(Raiz, "Logs");

        var contratosConfigurada = configuration["Armazenamento:PastaContratosGerados"];
        ContratosGerados = string.IsNullOrWhiteSpace(contratosConfigurada)
            ? Path.Combine(Raiz, "ContratosGerados")
            : contratosConfigurada;
    }

    public void GarantirEstrutura()
    {
        Directory.CreateDirectory(Raiz);
        Directory.CreateDirectory(Templates);
        Directory.CreateDirectory(ContratosGerados);
        Directory.CreateDirectory(Logs);
    }
}