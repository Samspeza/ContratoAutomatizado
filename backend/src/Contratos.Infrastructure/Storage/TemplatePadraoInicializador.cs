namespace Contratos.Infrastructure.Storage;

public static class TemplatePadraoInicializador
{
    public const string NomeArquivoTemplate = "modelo.docx";

    public static string GarantirTemplate(PastasAplicacao pastas)
    {
        var destino = Path.Combine(pastas.Templates, NomeArquivoTemplate);

        if (File.Exists(destino))
            return destino;

        var origem = Path.Combine(AppContext.BaseDirectory, "Templates", NomeArquivoTemplate);

        if (File.Exists(origem))
            File.Copy(origem, destino);

        return destino;
    }
}