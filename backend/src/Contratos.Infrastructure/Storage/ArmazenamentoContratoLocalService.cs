using Contratos.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Storage;

public sealed class ArmazenamentoContratoLocalService : IArmazenamentoContratoService
{
    private readonly string _pastaBase;

    public ArmazenamentoContratoLocalService(IConfiguration configuration)
    {
        _pastaBase = new PastasAplicacao(configuration).ContratosGerados;
        Directory.CreateDirectory(_pastaBase);
    }

    public string ObterPastaContratosGerados() => _pastaBase;

    public string DeterminarNomeBaseDisponivel(string cnpj)
    {
        var nomeBase = $"Contrato_{cnpj}";
        var contador = 2;

        while (ArquivoDocxOuPdfExiste(nomeBase))
        {
            nomeBase = $"Contrato_{cnpj}_v{contador}";
            contador++;
        }

        return nomeBase;
    }

    public string SalvarArquivo(byte[] conteudo, string nomeBase, string extensaoComPonto)
    {
        var caminho = Path.Combine(_pastaBase, $"{nomeBase}{extensaoComPonto}");
        File.WriteAllBytes(caminho, conteudo);
        return caminho;
    }

    private bool ArquivoDocxOuPdfExiste(string nomeBase)
    {
        return File.Exists(Path.Combine(_pastaBase, $"{nomeBase}.docx"))
            || File.Exists(Path.Combine(_pastaBase, $"{nomeBase}.pdf"));
    }
}