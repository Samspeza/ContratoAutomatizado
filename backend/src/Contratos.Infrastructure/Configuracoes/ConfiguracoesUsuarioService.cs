using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Contratos.Application.Interfaces;
using Contratos.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;

namespace Contratos.Infrastructure.Configuracoes;

public sealed class ConfiguracoesUsuarioService : IConfiguracoesUsuarioService
{
    private const string NomeArquivo = "configuracoes-usuario.json";
    private static readonly object Trava = new();

    private readonly IConfiguration _configuration;
    private readonly string _caminho;

    public static string CaminhoArquivo(PastasAplicacao pastas) => Path.Combine(pastas.Configuracoes, NomeArquivo);

    public ConfiguracoesUsuarioService(IConfiguration configuration)
    {
        _configuration = configuration;
        _caminho = CaminhoArquivo(new PastasAplicacao(configuration));
    }

    public void Definir(string chave, string? valor)
    {
        lock (Trava)
        {
            var raiz = Ler();
            var partes = chave.Split(':');

            var no = raiz;
            for (var i = 0; i < partes.Length - 1; i++)
            {
                if (no[partes[i]] is not JsonObject filho)
                {
                    filho = new JsonObject();
                    no[partes[i]] = filho;
                }
                no = filho;
            }

            var folha = partes[^1];
            if (string.IsNullOrWhiteSpace(valor))
                no.Remove(folha);
            else
                no[folha] = valor;

            Gravar(raiz);
        }

        // Recarrega na hora: sem isso, a mudança só apareceria depois de alguns instantes.
        (_configuration as IConfigurationRoot)?.Reload();
    }

    private JsonObject Ler()
    {
        if (!File.Exists(_caminho)) return new JsonObject();

        try
        {
            return JsonNode.Parse(File.ReadAllText(_caminho, Encoding.UTF8)) as JsonObject ?? new JsonObject();
        }
        catch (JsonException)
        {
            return new JsonObject();
        }
    }

    private void Gravar(JsonObject raiz)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_caminho)!);

        var opcoes = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var temporario = _caminho + ".tmp";
        File.WriteAllText(temporario, raiz.ToJsonString(opcoes), new UTF8Encoding(false));
        File.Move(temporario, _caminho, overwrite: true);
    }
}