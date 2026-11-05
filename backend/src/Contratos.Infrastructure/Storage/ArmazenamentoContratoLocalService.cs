using Contratos.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Storage;

public sealed class ArmazenamentoContratoLocalService : IArmazenamentoContratoService
{
    private readonly IConfiguration _configuration;

    public ArmazenamentoContratoLocalService(IConfiguration configuration)
    {
        _configuration = configuration;
        ObterPastaContratosGerados(); // garante a pasta já na inicialização
    }

    public string ObterPastaContratosGerados()
    {
        var pasta = new PastasAplicacao(_configuration).ContratosGerados;
        Directory.CreateDirectory(pasta);
        return pasta;
    }

    public string DeterminarNomeBaseDisponivel(string cnpj)
    {
        var pasta = ObterPastaContratosGerados();
        var nomeBase = $"Contrato_{cnpj}";
        var contador = 2;

        while (ArquivoDocxOuPdfExiste(pasta, nomeBase))
        {
            nomeBase = $"Contrato_{cnpj}_v{contador}";
            contador++;
        }

        return nomeBase;
    }

    public string SalvarArquivo(byte[] conteudo, string nomeBase, string extensaoComPonto)
    {
        var caminho = Path.Combine(ObterPastaContratosGerados(), $"{nomeBase}{extensaoComPonto}");
        File.WriteAllBytes(caminho, conteudo);
        return caminho;
    }

    private static bool ArquivoDocxOuPdfExiste(string pasta, string nomeBase)
    {
        return File.Exists(Path.Combine(pasta, $"{nomeBase}.docx"))
            || File.Exists(Path.Combine(pasta, $"{nomeBase}.pdf"));
    }
}